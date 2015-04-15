using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private static string SAVE_FILENAME = "DiginoteServer.state";
    private StreamWriter saveFile = null;

    private Logger logger; // log system

    public event ChangeDelegate ChangeEvent;    // event to warn clients 
                                                // when quotation changes
    
    private List<User> usersList; // users
    private List<User> loggedUsers; // à partida não é necessário


    private double quotation; // current quotation of diginotes

    private DiginoteDatabase diginoteDB; // diginote db

    private ArrayList sellOrders; // list of sell orders
    private ArrayList buyOrders; // list of buy orders

    public DiginoteTradingSystem()
    {
        Initialize();
        
        // if save file exists then load state
        if (File.Exists(SAVE_FILENAME))
            loadState();
        

        // 
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

    private void Initialize()
    {
        // set initial quotation
        quotation = 2.3;

        // create order lists
        buyOrders = new ArrayList();
        sellOrders = new ArrayList();
        usersList = new List<User>();
        loggedUsers = new List<User>();

        // create logger
        logger = new Logger();

    }

    public double GetQuotation()
    {
        return quotation;
    }

    private void SetNewQuotation(double value)
    {
        if(value > quotation) {
            Log("Quotation value went up to: " + value);
            SafeInvoke(new ChangeArgs(ChangeType.QuotationUp, value));
        }
        else {
            Log("Quotation value went down to: " + value);
            SafeInvoke(new ChangeArgs(ChangeType.QuotationDown, value));
        }

        // update quotation
        quotation = value;
        
    }

    public void SuggestNewQuotation(double value)
    {
        SetNewQuotation(value);
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
            Log("Register attempt failed from: " + newUser.Username);
            return false;
        }

        usersList.Add(newUser);
        Log("New user registered: " + newUser.Username);

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
            Log("Login attempt failed from: " + username);
            return new Pair<bool, User>(false, null);
        }

        User user = usersList.Find(tUser => tUser.Username == username);

        loggedUsers.Add(user);
        Log("User logged in: " + user.Username);

        return new Pair<bool, User>(true, user);
    }

    public List<User> getLoggedUsers()
    {
        return loggedUsers;
    }

    public void Logout(User loguser)
    {
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Log("User logged out: " + loguser.Username);
    }

    public int DiginotesFromUser(User user)
    {
        // return the number of diginotes that this user owns
        return 3;
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
        StreamReader reader = new StreamReader(SAVE_FILENAME);

        // load
        Console.WriteLine("[Server]: Loading server state");

        reader.Close();
    }

    private void saveState() 
    {
        if(saveFile == null)
            saveFile = new StreamWriter(SAVE_FILENAME);

        // save
    }

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Server]: Initialized Lifetime Service");
        return null;
    }
}