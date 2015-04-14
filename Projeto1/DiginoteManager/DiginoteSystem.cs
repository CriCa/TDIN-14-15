using System;
using System.Collections;
using System.Collections.Generic;

public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private double quotation; // current quotation of diginotes
    
    private List<User> usersList;
    private List<User> loggedUsers;

    private ArrayList sellOrders; // list of sell orders
    private ArrayList buyOrders; // list of buy orders

    private Logger logger; // log system

    public event ChangeDelegate ChangeEvent; // event to warn clients 
                                                    // when quotation changes

    private DiginoteDatabase diginoteDB; // diginote db

    public DiginoteTradingSystem()
    {
        // TODO check for log and load state

        // set initial quotation
        quotation = 1.0;

        // create order lists
        buyOrders = new ArrayList();
        sellOrders = new ArrayList();
        usersList = new List<User>();
        loggedUsers = new List<User>();

        // create logger
        logger = new Logger(ChangeEvent);
        Console.WriteLine("[DiginoteSystem] sup");
        diginoteDB = new DiginoteDatabase();

        // create brand new diginotes associated with a fictional user and sell them
        RegisterUser("System", "System", "Password");
        Diginote dig = new Diginote();
        for (int i = 0; i < 49; i++) {
            dig = new Diginote();
            diginoteDB.AddDiginote(dig, "System");
        }
        logger.Log("Diginotes Created.");
        AddSellOrder(new Order(OrderType.Sell, 50, "System"));


    }

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Server]: Initialized Lifetime Service");
        return null;
    }

    public double GetQuotation()
    {
        return quotation;
    }

    private void SetNewQuotation(double value)
    {
        if(value > quotation) {
            Console.WriteLine("[Server]: Quotation value went up to: " + value);
            ChangeEvent(new ChangeArgs(ChangeType.QuotationUp, value));
        }
        else {
            Console.WriteLine("[Server]: Quotation value went down to: " + value);
            ChangeEvent(new ChangeArgs(ChangeType.QuotationDown, value));
        }

        // update quotation
        quotation = value;
        
    }

    public void AddBuyOrder(Order newOrder)
    {
        Console.WriteLine("[Server]: Added buy order from user " + newOrder.User);
        buyOrders.Add(newOrder);
    }

    public void AddSellOrder(Order newOrder)
    {
        Console.WriteLine("[Server]: Added sell order from user " + newOrder.User);
        sellOrders.Add(newOrder);
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
            Console.WriteLine("[DiginoteSystem] register failed: {0}", newUser.Username);
            return false;
        }

        usersList.Add(newUser);
        Console.WriteLine("[DiginoteSystem] user registered: {0}", newUser.Username);

        // SAVE STATE

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
            Console.WriteLine("[DiginoteSystem] login failed in: {0}", username);
            return new Pair<bool, User>(false, null);
        }

        User user = usersList.Find(tUser => tUser.Username == username);

        loggedUsers.Add(user);
        Console.WriteLine("[DiginoteSystem] user logged in: {0}", user.Username);

        return new Pair<bool, User>(true, user);
    }

    public List<User> getLoggedUsers()
    {
        return loggedUsers;
    }

    public void Logout(User loguser)
    {
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Console.WriteLine("[DiginoteSystem] user logged out: {0}", loguser.Username);
    }

    // this function must be called to when something occurs
    //  and we need to call event, the thread is needed to
    //  prevent dead locks on server and client!
    private void safeInvoke(ChangeArgs args)
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
}