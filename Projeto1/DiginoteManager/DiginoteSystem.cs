using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;

public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private static string SAVE_FILENAME = "DiginoteServer.state";

    private Logger logger; // log system

    public event ChangeDelegate ChangeEvent;    // event to warn clients 
    // when quotation changes

    private List<User> usersList; // users
    private List<User> loggedUsers; // users logged in

    private double quotation; // current quotation of diginotes

    private List<Diginote> diginoteDB; // diginote db

    private List<Order> finishedOrders; // list of finished orders (over or removed)
    private List<Order> sellOrders; // list of sell orders
    private List<Order> buyOrders; // list of buy orders

    private List<Pair<DateTime, double>> quotationEvolution;

    private List<Pair<DateTime, int>> transactionsPerMin;
    
    public DiginoteTradingSystem()
    {
        Initialize();

        // if save file exists then load state
        if (File.Exists(SAVE_FILENAME))
            loadState();
    }

    private void Initialize()
    {
        // set initial quotation
        quotation = 1.0;

        // create order lists
        finishedOrders = new List<Order>();
        buyOrders = new List<Order>();
        sellOrders = new List<Order>();

        // create users lists
        usersList = new List<User>();
        loggedUsers = new List<User>();

        // create diginotes list
        diginoteDB = new List<Diginote>();

        // create statistics lists
        quotationEvolution = new List<Pair<DateTime, double>>();
        quotationEvolution.Add(new Pair<DateTime, double>(DateTime.Now, quotation));
        transactionsPerMin = new List<Pair<DateTime, int>>();

        // create logger
        logger = new Logger();
    }

    public double GetQuotation() { return quotation; }

    public void SuggestNewQuotation(User user, double value) { SetNewQuotation(user, value); }

    private void SetNewQuotation(User user, double value)
    {
        Pair<DateTime, double> stat = new Pair<DateTime, double>(DateTime.Now, value);
        quotationEvolution.Add(stat);

        if (value > quotation)
        {
            Log("Quotation value went up to: " + value);

            SafeInvoke(new ChangeArgs(ChangeType.QuotationUp, value, user.Username, stat));

            foreach (Order order in buyOrders)
                if (order.User.Username != user.Username && order.State == OrderState.Pending)
                    order.State = OrderState.WaitApproval;
        }
        else
        {
            Log("Quotation value went down to: " + value);

            SafeInvoke(new ChangeArgs(ChangeType.QuotationDown, value, user.Username, stat));

            foreach (Order order in sellOrders)
                if (order.User.Username != user.Username && order.State == OrderState.Pending)
                    order.State = OrderState.WaitApproval;
        }

        // update quotation
        quotation = value;

        // save state
        saveState();
    }

    public Tuple<int, int, int, int, int> GetSystemInfo()
    {
        return new Tuple<int, int, int, int, int>(usersList.Count, loggedUsers.Count, diginoteDB.Count, CountDiginotesOffer(), CountDiginotesDemmand());
    }

    private int CountDiginotesDemmand()
    {
        int result = 0;
        foreach (Order order in buyOrders)
            result += order.Quantity;
        return result;
    }

    private int CountDiginotesOffer()
    {
        int result = 0;
        foreach (Order order in sellOrders)
            result += order.Quantity;
        return result;
    }

    public Order AddBuyOrder(User user, int quantity, OrderType orderType)
    {
        // create new order
        Order newOrder = new Order(orderType, quantity, user);

        // add to sell orders
        Log("Added buy order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");

        // see if can make a transaction
        HandleOrder(newOrder, buyOrders, sellOrders);

        // save state
        saveState();

        SafeInvoke(new ChangeArgs(CountDiginotesOffer(), CountDiginotesDemmand()));

        return newOrder;
    }

    public Order AddSellOrder(User user, int quantity, OrderType orderType)
    {
        // create new order
        Order newOrder = new Order(orderType, quantity, user);

        // add to sell orders
        Log("Added sell order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");

        // see if can make a transaction
        HandleOrder(newOrder, sellOrders, buyOrders);

        // save state
        saveState();

        SafeInvoke(new ChangeArgs(CountDiginotesOffer(), CountDiginotesDemmand()));

        return newOrder;
    }

    private void HandleOrder(Order newOrder, List<Order> sameTypeList, List<Order> otherTypeList) 
    {
        if (otherTypeList.Count == 0)
            sameTypeList.Add(newOrder);
        else
        {
            foreach (Order order in otherTypeList)
            {
                if (order.State == OrderState.Pending)
                {
                    if (newOrder.Type == OrderType.Buy)
                        Transaction(order, newOrder, Math.Min(newOrder.Quantity, order.Quantity));
                    else
                        Transaction(newOrder, order, Math.Min(newOrder.Quantity, order.Quantity));
                }

                if (newOrder.Quantity == 0)
                    break;
            }

            foreach (Order order in otherTypeList.FindAll(o => o.Quantity == 0))
            {
                otherTypeList.Remove(order);
                finishedOrders.Add(order);
            }

            if (newOrder.Quantity == 0)
                finishedOrders.Add(newOrder);
            else
                sameTypeList.Add(newOrder);
        }
    }

    private void Transaction(Order from, Order to, int quantity)
    {
        List<DiginoteInfo> digInfo = new List<DiginoteInfo>();
        List<Diginote> diginotesFrom = diginoteDB.FindAll(d => d.Owner.Username == from.User.Username);

        if (diginotesFrom.Count >= quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                Diginote dig = diginotesFrom[i];
                dig.Owner = to.User;
                digInfo.Add(new DiginoteInfo(dig.Id, dig.Value, dig.LastAquiredOn));
            }

            from.Quantity -= quantity;
            to.Quantity -= quantity;

            Log(quantity + "　Diginotes transacted from " + from.User.Username + " to " + to.User.Username + " at " + quotation + "$");

            
            if (transactionsPerMin.Count > 0) { 
                Pair<DateTime, int> lastStatItem = transactionsPerMin[transactionsPerMin.Count - 1];
                if (lastStatItem.first.ToString("dd/MM/yyyy hh:mm") == DateTime.Now.ToString("dd/MM/yyyy hh:mm"))
                    lastStatItem.second++;
                else
                    transactionsPerMin.Add(new Pair<DateTime,int>(DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy hh:mm")), 1));
            }
            else transactionsPerMin.Add(new Pair<DateTime, int>(DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy hh:mm")), 1));

            SafeInvoke(new ChangeArgs(from, to, digInfo, transactionsPerMin[transactionsPerMin.Count - 1]));
        }
    }

    public Order RemoveOrder(User user, Order order)
    {
        Order removedOrder;

        if(order.Type == OrderType.Buy)
        {
            // find order in buy orders list and remove it
            removedOrder = buyOrders.Find(buyOrder => buyOrder.User.Username == user.Username);
            buyOrders.Remove(removedOrder);
        }
        else
        {
            // find order in buy orders list and remove it
            removedOrder = sellOrders.Find(sellOrder => sellOrder.User.Username == user.Username);
            sellOrders.Remove(removedOrder);
        }

        // change state and add to finished orders
        removedOrder.State = OrderState.Removed;
        finishedOrders.Add(removedOrder);

        Log("Removed order from user " + user.Username);

        // save state
        saveState();

        SafeInvoke(new ChangeArgs(CountDiginotesOffer(), CountDiginotesDemmand()));

        return removedOrder;
    }

    public Order ReceiveApproval(User user, Order order, bool approve)
    {
        if (!approve)
            return RemoveOrder(user, order);
        else
        {
            Order retOrder;

            if (order.Type == OrderType.Buy)
            {
                retOrder = buyOrders.Find(o => o.User.Username == user.Username);
                retOrder.State = OrderState.Pending;
                HandleOrder(retOrder, buyOrders, sellOrders);
            }
            else
            {
                retOrder = sellOrders.Find(o => o.User.Username == user.Username);
                retOrder.State = OrderState.Pending;
                HandleOrder(retOrder, sellOrders, buyOrders);
            }

            saveState();

            return retOrder;
        }
    }

    private bool UserExists(User newUser)
    {
        return usersList.Exists(user => user.Username == newUser.Username);
    }

    public bool RegisterUser(string name, string username, string password)
    {
        User newUser = new User(name, username, password);

        if (UserExists(newUser))
        {
            Log("Register attempt failed from: " + newUser.Username);
            return false;
        }

        usersList.Add(newUser);
        Log("New user registered: " + newUser.Username);

        for (int i = 0; i < 5; i++)
            diginoteDB.Add(new Diginote(newUser));

        // save state
        saveState();

        return true;
    }

    private bool CheckLogin(string username, string password)
    {
        return usersList.Exists(user => user.Username == username && user.Password == password)
            && !loggedUsers.Exists(loggedUser => loggedUser.Username == username);
    }

    public Pair<bool, User> Login(string username, string password) // return is equivalent to pair
    {
        if (!CheckLogin(username, password))
        {
            Log("Login attempt failed from: " + username);
            return new Pair<bool, User>(false, null);
        }

        User user = usersList.Find(tUser => tUser.Username == username);

        loggedUsers.Add(user);

        Log("User logged in: " + user.Username);

        SafeInvoke(new ChangeArgs(usersList.Count, loggedUsers.Count, diginoteDB.Count));

        return new Pair<bool, User>(true, user);
    }

    public void Logout(User loguser)
    {
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Log("User logged out: " + loguser.Username);

        SafeInvoke(new ChangeArgs(ChangeType.Logout, loggedUsers.Count));
    }

    public List<DiginoteInfo> DiginotesFromUser(User user)
    {
        List<DiginoteInfo> digs = new List<DiginoteInfo>();

        diginoteDB.FindAll(dig => dig.Owner.Username == user.Username)
            .ForEach(digInf => digs.Add(new DiginoteInfo(digInf.Id, digInf.Value, digInf.LastAquiredOn)));

        return digs;
    }

    public List<Order> OrdersFromUser(User user)
    {
        List<Order> orders = new List<Order>();

        // get orders of user from all lists
        finishedOrders.FindAll(order => order.User.Username == user.Username)
            .ForEach(orderAux => orders.Add(orderAux));

        sellOrders.FindAll(order => order.User.Username == user.Username)
            .ForEach(orderAux => orders.Add(orderAux));

        buyOrders.FindAll(order => order.User.Username == user.Username)
            .ForEach(orderAux => orders.Add(orderAux));

        // sort if necessary
        orders.Sort(
            delegate(Order p1, Order p2)
            {
                return DateTime.Parse(p1.CreatedOn).CompareTo(DateTime.Parse(p2.CreatedOn));
            });

        return orders;
    }

    public double GetDigtime()
    {
        return diginoteDB.Count * 2;
    }

    public DiginoteInfo DigDiginote(User user)
    {
        // create new diginote
        Diginote dig = new Diginote(user, 1.0 + diginoteDB.Count * 0.05);

        // add diginote to list
        diginoteDB.Add(dig);
        Log(user.Username + " digged a diginote");

        // save state
        saveState();

        SafeInvoke(new ChangeArgs(ChangeType.SysDiginotes, diginoteDB.Count));

        return new DiginoteInfo(dig.Id, dig.Value, dig.LastAquiredOn);
    }

    public List<Pair<DateTime, double>> GetQuotationEvolution() { return quotationEvolution; }

    public List<Pair<DateTime, int>> GetTransactionsPerMin() { return transactionsPerMin; }

    // this function must be called to when something occurs
    //  and we need to call event, the thread is needed to
    //  prevent dead locks on server and client!
    private void SafeInvoke(ChangeArgs args)
    {
        if (ChangeEvent != null)
        {
            Delegate[] invocationList = ChangeEvent.GetInvocationList();

            foreach (ChangeDelegate changeDelegate in invocationList)
            {
                try
                {
                    changeDelegate.BeginInvoke(args, null, null);
                }
                catch (Exception)
                {
                    ChangeEvent -= changeDelegate;
                }
            }
        }
    }

    private void Log(string msg)
    {
        logger.Log(msg);
        Console.WriteLine("[Server]: " + msg);
    }

    private void loadState()
    {
        try
        {
            // open stream
            Stream stream = File.Open(SAVE_FILENAME, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();

            // load from file to state
            Tuple<
                List<User>, // users
                double, // quotation
                List<Diginote>, // diginotes
                int, // diginote serial
                Tuple<List<Order>, List<Order>, List<Order>>, // finished, sell and buy orders
                List<Pair<DateTime, double>>,
                List<Pair<DateTime, int>>
                > state =
                (Tuple<List<User>, double, List<Diginote>, int, Tuple<List<Order>, List<Order>, List<Order>>, List<Pair<DateTime, double>>, List<Pair<DateTime, int>>>)formatter.Deserialize(stream);
            
            // users list
            usersList = state.Item1;
            // quotation
            quotation = state.Item2;
            // diginotes
            diginoteDB = state.Item3;
            Diginote.NextSerial = state.Item4;
            // orders
            finishedOrders = state.Item5.Item1;
            sellOrders = state.Item5.Item2;
            buyOrders = state.Item5.Item3;
            // stats
            quotationEvolution = state.Item6;
            transactionsPerMin = state.Item7;

            Log("State loaded");
            stream.Close();
        }
        catch (Exception e)
        {
            Log("Error loading state");
            Log(e.Message);
        }
    }

    private void saveState()
    {
        try
        {
            // open stream
            Stream stream = File.Open(SAVE_FILENAME, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            // create state
            Tuple<
                List<User>, // users
                double, // quotation
                List<Diginote>, // diginotes
                int, // diginote serial
                Tuple<List<Order>, List<Order>, List<Order>>, // finished, sell and buy orders
                List<Pair<DateTime, double>>,
                List<Pair<DateTime, int>>
                > state =
                new Tuple<List<User>, double, List<Diginote>, int, Tuple<List<Order>, List<Order>, List<Order>>, List<Pair<DateTime, double>>, List<Pair<DateTime, int>>>
                    (usersList, quotation, diginoteDB, Diginote.NextSerial, 
                    new Tuple<List<Order>, List<Order>, List<Order>>(finishedOrders, sellOrders, buyOrders),
                    quotationEvolution, transactionsPerMin);

            // save to file and close stream
            formatter.Serialize(stream, state);
            stream.Close();
        }
        catch (Exception e)
        {
            Log("Error saving state");
            Log(e.Message);
        }
    }

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Server]: Initialized Lifetime Service");
        return null;
    }
}