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
    private List<User> loggedUsers; // à partida não é necessário

    private double quotation; // current quotation of diginotes

    private List<Diginote> diginoteDB; // diginote db

    private List<Order> sellOrders; // list of sell orders
    private List<Order> buyOrders; // list of buy orders

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

    public void ReceiveApproval(User user, bool appr, OrderType orderType)
    {
        if (!appr)
            RemoveOrder(user);
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

    private void RemoveOrder(User user)
    {
        //throw new NotImplementedException();
    }

    public Order AddBuyOrder(User user, int quantity, OrderType orderType)
    {
        Order newOrder = new Order(orderType, quantity, user);
        
        Log("Added buy order from user " + newOrder.User.Username + " of " + newOrder.Quantity + " Diginotes");
        
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
        return usersList.Exists(user => user.Username == username && user.Password == password);
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

        return new Pair<bool, User>(true, user);
    }

    public void Logout(User loguser)
    {
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Log("User logged out: " + loguser.Username);
    }

    public List<DiginoteInfo> DiginotesFromUser(User user)
    {
        List<DiginoteInfo> digs = new List<DiginoteInfo>();

        diginoteDB.FindAll(dig => dig.Owner.Username == user.Username)
            .ForEach(digInf => digs.Add(new DiginoteInfo(digInf.Id, digInf.Value, digInf.LastAquiredOn)));

        return digs;
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