using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;

/**
 * Class where is the main logic of the server, responsible for holding information
 * about users, their diginotes and make transactions
 */
public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private static string SAVE_FILENAME = "DiginoteServer.state"; // save state file name

    private Logger logger; // log system

    public event ChangeDelegate ChangeEvent; // event to keep clients updated of changes in server

    private List<User> usersList; // users in system
    private List<User> loggedUsers; // users logged in

    private double quotation; // current quotation

    private List<Diginote> diginoteDB; // diginote database

    private List<Order> finishedOrders; // list of finished orders (over or removed)
    private List<Order> sellOrders; // list of sell orders
    private List<Order> buyOrders; // list of buy orders

    private List<Pair<DateTime, double>> quotationEvolution; // statistics of quotation value in time

    private List<Pair<DateTime, int>> transactionsPerMin; // statistics of transactions made per minute in time
    
    // constructor
    public DiginoteTradingSystem()
    {
        // initialize data
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

    // get current quotation
    public double GetQuotation() { return quotation; }

    // suggest new quotation by user
    public void SuggestNewQuotation(User user, double value) { SetNewQuotation(user, value); }

    // function that sets the new quotation and warns clients about the change
    private void SetNewQuotation(User user, double value)
    {
        // new statistics information
        Pair<DateTime, double> stat = new Pair<DateTime, double>(DateTime.Now, value);
        // add to statistics list
        quotationEvolution.Add(stat);

        if (value > quotation) // if quotation gone up
        {
            Log("Quotation value went up to: " + value);

            // warn clients about new quotation
            SafeInvoke(new ChangeArgs(ChangeType.QuotationUp, value, user.Username, stat));

            // freeze buy orders from other users
            foreach (Order order in buyOrders)
                if (order.User.Username != user.Username && order.State == OrderState.Pending)
                    order.State = OrderState.WaitApproval;
        }
        else
        {
            Log("Quotation value went down to: " + value);

            // warn clients about new quotation
            SafeInvoke(new ChangeArgs(ChangeType.QuotationDown, value, user.Username, stat));

            // freeze sell orders from other users
            foreach (Order order in sellOrders)
                if (order.User.Username != user.Username && order.State == OrderState.Pending)
                    order.State = OrderState.WaitApproval;
        }

        // update quotation
        quotation = value;

        // save state
        saveState();
    }

    // get general system info
    public Tuple<int, int, int, int, int> GetSystemInfo()
    {
        return new Tuple<int, int, int, int, int>(usersList.Count, loggedUsers.Count, diginoteDB.Count, CountDiginotesOffer(), CountDiginotesDemmand());
    }

    // function that counts the number of diginotes that clients want to buy
    private int CountDiginotesDemmand()
    {
        int result = 0;
        foreach (Order order in buyOrders)
            result += order.Quantity;
        return result;
    }

    // function that counts the number of diginotes that clients want to sell
    private int CountDiginotesOffer()
    {
        int result = 0;
        foreach (Order order in sellOrders)
            result += order.Quantity;
        return result;
    }

    // function that adds a new buy order from a user and make transactions if possible
    public Order AddBuyOrder(User user, int quantity, OrderType orderType)
    {
        // create new order
        Order newOrder = new Order(orderType, quantity, user);

        Log("Added buy order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");

        // see if can make a transaction and performs it if it can
        HandleOrder(newOrder, buyOrders, sellOrders);

        // save state
        saveState();

        // warn clients about changes
        SafeInvoke(new ChangeArgs(CountDiginotesOffer(), CountDiginotesDemmand()));

        return newOrder;
    }

    // function that adds a new sell order from a user and make transactions if possible
    public Order AddSellOrder(User user, int quantity, OrderType orderType)
    {
        // create new order
        Order newOrder = new Order(orderType, quantity, user);

        Log("Added sell order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");

        // see if can make a transaction and performs it if it can
        HandleOrder(newOrder, sellOrders, buyOrders);

        // save state
        saveState();

        // warn clients about changes
        SafeInvoke(new ChangeArgs(CountDiginotesOffer(), CountDiginotesDemmand()));

        return newOrder;
    }

    // function that check if can make transactions in the current state and performs them if it can
    private void HandleOrder(Order newOrder, List<Order> sameTypeList, List<Order> otherTypeList) 
    {
        // if the other list is empty is not possible to make a transaction
        if (otherTypeList.Count == 0)
            sameTypeList.Add(newOrder); // then add the order to the list so it can be handled after
        else
        {
            // for each order of opposite type that is pending (not on hold) make transaction
            foreach (Order order in otherTypeList)
            {
                if (order.State == OrderState.Pending)
                {
                    if (newOrder.Type == OrderType.Buy)
                        Transaction(order, newOrder, Math.Min(newOrder.Quantity, order.Quantity));
                    else
                        Transaction(newOrder, order, Math.Min(newOrder.Quantity, order.Quantity));
                }

                // if this order is satisfied then break out of the cicle
                if (newOrder.Quantity == 0)
                    break;
            }

            // remove satistied orders in the others list and adds them to the finished list
            foreach (Order order in otherTypeList.FindAll(o => o.Quantity == 0))
            {
                otherTypeList.Remove(order);
                finishedOrders.Add(order);
            }

            if (newOrder.Quantity == 0) // if satisfied then add to finished list
                finishedOrders.Add(newOrder);
            else // else add the order to the list so it can be handled after
                sameTypeList.Add(newOrder);
        }
    }

    // function that performs a transaction
    private void Transaction(Order from, Order to, int quantity)
    {
        List<DiginoteInfo> digInfo = new List<DiginoteInfo>();

        // get all diginotes from the user that is selling diginotes
        List<Diginote> diginotesFrom = diginoteDB.FindAll(d => d.Owner.Username == from.User.Username);

        if (diginotesFrom.Count >= quantity)
        {
            // iterate over 'quantity' diginotes and change owner to the user that is buying diginotes
            for (int i = 0; i < quantity; i++)
            {
                Diginote dig = diginotesFrom[i];
                dig.Owner = to.User;
                digInfo.Add(new DiginoteInfo(dig.Id, dig.Value, dig.LastAquiredOn));
            }

            // update quantity in both orders
            from.Quantity -= quantity;
            to.Quantity -= quantity;

            Log(quantity + "　Diginotes transacted from " + from.User.Username + " to " + to.User.Username + " at " + quotation + "$");
            
            // set transaction statistics
            if (transactionsPerMin.Count > 0) { 
                Pair<DateTime, int> lastStatItem = transactionsPerMin[transactionsPerMin.Count - 1];
                if (lastStatItem.first.ToString("dd/MM/yyyy hh:mm") == DateTime.Now.ToString("dd/MM/yyyy hh:mm"))
                    lastStatItem.second++;
                else
                    transactionsPerMin.Add(new Pair<DateTime,int>(DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy hh:mm")), 1));
            }
            else transactionsPerMin.Add(new Pair<DateTime, int>(DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy hh:mm")), 1));

            // warn clients about the transaction to update information in the client
            SafeInvoke(new ChangeArgs(from, to, digInfo, transactionsPerMin[transactionsPerMin.Count - 1]));
        }
    }

    // function that removes a order from the user
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

        // warn clients about the new offer and demand count
        SafeInvoke(new ChangeArgs(CountDiginotesOffer(), CountDiginotesDemmand()));

        return removedOrder;
    }

    // function that handles the approval or disapproval of a user to keep an order after a quotation change
    public Order ReceiveApproval(User user, Order order, bool approve)
    {
        if (!approve) // if the user doesn't approve then remove the order
            return RemoveOrder(user, order);
        else
        {
            Order retOrder;

            if (order.Type == OrderType.Buy) // if the order is of type buy
            {
                // get order in buy orders
                retOrder = buyOrders.Find(o => o.User.Username == user.Username);
                // change state to pending
                retOrder.State = OrderState.Pending;
                // see if can make a transaction and performs it if it can
                HandleOrder(retOrder, buyOrders, sellOrders);
            }
            else  // if the order is of type sell
            {
                // get order in sell orders
                retOrder = sellOrders.Find(o => o.User.Username == user.Username);
                // change state to pending
                retOrder.State = OrderState.Pending;
                // see if can make a transaction and performs it if it can
                HandleOrder(retOrder, sellOrders, buyOrders);
            }

            // save state
            saveState();

            return retOrder;
        }
    }

    // function that checks if a username already exists
    private bool UserExists(User newUser) { return usersList.Exists(user => user.Username == newUser.Username); }

    // function that regists a new user in the system return true in case of success and false otherwise
    public bool RegisterUser(string name, string username, string password)
    {
        // create new user
        User newUser = new User(name, username, password);

        // check if username already exists
        if (UserExists(newUser))
        {
            Log("Register attempt failed from: " + newUser.Username);
            return false;
        }

        // add user to the list
        usersList.Add(newUser);
        Log("New user registered: " + newUser.Username);

        // give the user 5 diginotes
        for (int i = 0; i < 5; i++)
            diginoteDB.Add(new Diginote(newUser));

        // save state
        saveState();

        return true;
    }

    // function that check if the login info of an user is correct and returns true or false
    private bool CheckLogin(string username, string password)
    {
        return usersList.Exists(user => user.Username == username && user.Password == password)
            && !loggedUsers.Exists(loggedUser => loggedUser.Username == username);
    }

    // function that logins an user and returns an instance of 'User' class in case of success
    public Pair<bool, User> Login(string username, string password) // return is equivalent to pair
    {
        // if login info isn't correct
        if (!CheckLogin(username, password))
        {
            Log("Login attempt failed from: " + username); // log atempt failed
            return new Pair<bool, User>(false, null); // return false and a null instance
        }

        // get user instance
        User user = usersList.Find(tUser => tUser.Username == username);

        // add user to the logged in  list
        loggedUsers.Add(user);
        Log("User logged in: " + user.Username);

        // warn clients about the new counts
        SafeInvoke(new ChangeArgs(usersList.Count, loggedUsers.Count, diginoteDB.Count));

        return new Pair<bool, User>(true, user);
    }

    // function that logouts a user
    public void Logout(User loguser)
    {
        // remove user from logged in list
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Log("User logged out: " + loguser.Username);

        // warn clients about the new users logged in count
        SafeInvoke(new ChangeArgs(ChangeType.Logout, loggedUsers.Count));
    }

    // function that returns a list with the diginotes info that are owned by an user
    public List<DiginoteInfo> DiginotesFromUser(User user)
    {
        List<DiginoteInfo> digs = new List<DiginoteInfo>();

        // get all diginotes from an user
        diginoteDB.FindAll(dig => dig.Owner.Username == user.Username)
            .ForEach(digInf => digs.Add(new DiginoteInfo(digInf.Id, digInf.Value, digInf.LastAquiredOn)));

        return digs;
    }

    // function that returns a list of the orders made by an user
    public List<Order> OrdersFromUser(User user)
    {
        List<Order> orders = new List<Order>();

        // get orders of user from all lists and append to orders
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

    // function that returns the time necessary to dig a new diginote
    public double GetDigtime() { return diginoteDB.Count * 2; }

    // function that adds a new diginote to user and return info about the new diginote
    public DiginoteInfo DigDiginote(User user)
    {
        // create new diginote with a value that depends on the number of diginotes that already exist
        Diginote dig = new Diginote(user, 1.0 + diginoteDB.Count * 0.05);

        // add diginote to list
        diginoteDB.Add(dig);
        Log(user.Username + " digged a diginote");

        // save state
        saveState();

        // warn clients about the new diginote count
        SafeInvoke(new ChangeArgs(ChangeType.SysDiginotes, diginoteDB.Count));

        return new DiginoteInfo(dig.Id, dig.Value, dig.LastAquiredOn);
    }

    // getter for quotation statistics
    public List<Pair<DateTime, double>> GetQuotationEvolution() { return quotationEvolution; }

    // getter for transactions statistics
    public List<Pair<DateTime, int>> GetTransactionsPerMin() { return transactionsPerMin; }

    // function that invokes the event in order to warn clients about server changes without creating a deadlock
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

    // wrapper function that logs a message and prints it to console
    private void Log(string msg) { logger.Log(msg); Console.WriteLine("[Server]: " + msg); }

    // function that loads the server state from a file
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
                List<Pair<DateTime, double>>, // statistics for quotation
                List<Pair<DateTime, int>> // statistics for transactions
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
            // statitics
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

    // function that saves the server state to a file
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
                List<Pair<DateTime, double>>, // statistics for quotation
                List<Pair<DateTime, int>> // statistics for transactions
                > state =
                new Tuple<List<User>, double, List<Diginote>, int, Tuple<List<Order>, List<Order>, List<Order>>, List<Pair<DateTime, double>>, List<Pair<DateTime, int>>>
                    (usersList, quotation, diginoteDB, Diginote.NextSerial, 
                    new Tuple<List<Order>, List<Order>, List<Order>>(finishedOrders, sellOrders, buyOrders),
                    quotationEvolution, transactionsPerMin);

            // save to file
            formatter.Serialize(stream, state);
            // close stream
            stream.Close();
        }
        catch (Exception e)
        {
            Log("Error saving state");
            Log(e.Message);
        }
    }

    // overiding lifetime to infinite
    public override object InitializeLifetimeService() { Console.WriteLine("[Server]: Initialized Lifetime Service"); return null; }
}