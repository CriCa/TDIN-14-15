using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

    private List<Order> sellOrders; // list of sell orders
    private List<Order> buyOrders; // list of buy orders

    // TRANSACTION -> SAFEINVOKE TRANSACTION
    
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
        buyOrders = new List<Order>();
        sellOrders = new List<Order>();

        usersList = new List<User>();
        loggedUsers = new List<User>();

        diginoteDB = new List<Diginote>();

        // create logger
        logger = new Logger();
    }

    public double GetQuotation() { return quotation; }

    public void SuggestNewQuotation(User user, double value) { SetNewQuotation(user, value); }

    private void SetNewQuotation(User user, double value)
    {
        if (value > quotation)
        {
            Log("Quotation value went up to: " + value);

            SafeInvoke(new ChangeArgs(ChangeType.QuotationUp, value, user.Username));

            foreach (Order order in buyOrders)
                if (order.User.Username != user.Username)
                    order.State = OrderState.WaitApproval;
        }
        else
        {
            Log("Quotation value went down to: " + value);

            SafeInvoke(new ChangeArgs(ChangeType.QuotationDown, value, user.Username));

            foreach (Order order in sellOrders)
                if (order.User.Username != user.Username)
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
        foreach(Order order in buyOrders)
            result += order.Quantity;
        return result;
    }

    private int CountDiginotesOffer()
    {
        int result = 0;
        foreach(Order order in sellOrders)
            result += order.Quantity;
        return result;
    }

    public void ReceiveApproval(User user, bool appr, OrderType orderType)
    {
        if (!appr)
            RemoveOrder(user,orderType);
        else
            if (orderType == OrderType.Buy)
            {
                bool allPending = true;
                foreach (Order order in buyOrders)
                {
                    if (order.User == user)
                    {
                        order.State = OrderState.Pending;
                    }
                    if (order.State != OrderState.Pending)
                        allPending = false;
                }
                //if(allPending) handle them


            }
            else if (orderType == OrderType.Sell)
            {
                bool allPending = true;
                foreach (Order order in sellOrders)
                {
                    if (order.User == user)
                    {
                        order.State = OrderState.Pending;
                    }
                    if (order.State != OrderState.Pending)
                        allPending = false;
                }
                //if(allPending) handle them
            }


        //        else change order state to pending

    }

    private void RemoveOrder(User user, OrderType orderType)
    {
         if (orderType == OrderType.Buy)
         {
                foreach (Order order in buyOrders)
                {
                    if (order.User == user)
                        buyOrders.Remove(order);
                }

            }
         else if (orderType == OrderType.Sell)
         {
             foreach (Order order in sellOrders)
             {
                 if (order.User == user)
                     sellOrders.Remove(order);
             }
         }
    }

    public Order AddBuyOrder(User user, int quantity, OrderType orderType)
    {
        Order newOrder = new Order(orderType, quantity, user);
        
        Log("Added buy order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");
        
        //If can't handle order put it in the list.
        if(!handleBuyOrder(newOrder))
            buyOrders.Add(newOrder);
        
        return newOrder;
    }

    public Order AddSellOrder(User user, int quantity, OrderType orderType)
    {
        Order newOrder = new Order(orderType, quantity, user);
        newOrder.State = OrderState.Pending;
        
        Log("Added sell order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");
        
        sellOrders.Add(newOrder);
        
        return newOrder;
    }

    private bool handleBuyOrder(Order newOrder)
    {
        int count;
        foreach (Order order in sellOrders)
        {
            if(order.State == OrderState.Pending)
                {
                    count = newOrder.Quantity;
                    if (order.Quantity > newOrder.Quantity)
                    {
                        List<DiginoteInfo> diginfo = makeTransaction(order,newOrder,count);
                        SafeInvoke(new ChangeArgs(order.User.Username,newOrder.User.Username, diginfo));
                        return true;
                    }
                    else if (order.Quantity == newOrder.Quantity)
                    {
                        List<DiginoteInfo> diginfo = makeTransaction(order, newOrder, count);
                        SafeInvoke(new ChangeArgs(order.User.Username, newOrder.User.Username, diginfo));
                        //ENVIAR ORDER QUANDO REMOVIDA?
                        sellOrders.Remove(order);
                        order.State = OrderState.Over;
                        return true;
                    }
                    else
                    {
                        count = order.Quantity;
                        List<DiginoteInfo> diginfo = makeTransaction(order, newOrder, count);
                        SafeInvoke(new ChangeArgs(order.User.Username, newOrder.User.Username, diginfo));
                        sellOrders.Remove(order);
                        order.State = OrderState.Over;
                        return true;
                    }
                }
        }
        
        return false;
    }

    private List<DiginoteInfo> makeTransaction(Order from, Order to,int count)
    {
        List<DiginoteInfo> diginfo = new List<DiginoteInfo>();
        Log(count+"　diginotes transacted from "+from.User.Username+" to "+to.User.Username+".");
        foreach (Diginote dig in diginoteDB)
        {
            if (dig.Owner == from.User)
            {
                count--;
                dig.LastAquiredOn = DateTime.Now.ToString();
                dig.Owner = to.User;
                diginfo.Add(new DiginoteInfo(dig.Id, dig.Value, dig.LastAquiredOn));
            }
            if (count == 0)
                break;
        }
        from.Quantity -= to.Quantity;
        
        return diginfo;
    }

    private bool userExists(User newUser)
    {
        return usersList.Exists(user => user.Username == newUser.Username);
    }

    public bool RegisterUser(string name, string username, string password)
    {
        User newUser = new User(name, username, password);

        if (userExists(newUser))
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

    private bool checkLogin(string username, string password)
    {
        return usersList.Exists(user => user.Username == username && user.Password == password) 
            && !loggedUsers.Exists(loggedUser => loggedUser.Username == username);
    }

    public Pair<bool, User> Login(string username, string password) // return is equivalent to pair
    {
        if (!checkLogin(username, password))
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

    public double GetDigtime()
    {
        return diginoteDB.Count * 2;
    }

    public DiginoteInfo DigDiginote(User user)
    {
        Diginote dig = new Diginote(user);

        diginoteDB.Add(dig);

        saveState();

        SafeInvoke(new ChangeArgs(ChangeType.SysDiginotes, diginoteDB.Count));
        
        return new DiginoteInfo(dig.Id, dig.Value, dig.LastAquiredOn);
    }

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
            Stream stream = File.Open(SAVE_FILENAME, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            Tuple<List<User>, double, List<Diginote>, List<Order>, List<Order>, int> state =
                (Tuple<List<User>, double, List<Diginote>, List<Order>, List<Order>, int>)formatter.Deserialize(stream);
            usersList = state.Item1;
            quotation = state.Item2;
            diginoteDB = state.Item3;
            sellOrders = state.Item4;
            buyOrders = state.Item5;
            Diginote.NextSerial = state.Item6;

            Log("State loaded.");
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
            Stream stream = File.Open(SAVE_FILENAME, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            Tuple<List<User>, double, List<Diginote>, List<Order>, List<Order>, int> state =
                new Tuple<List<User>, double, List<Diginote>, List<Order>, List<Order>, int>(usersList, quotation, diginoteDB, sellOrders, buyOrders, Diginote.NextSerial);

            formatter.Serialize(stream, state);
            Log("State saved");
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