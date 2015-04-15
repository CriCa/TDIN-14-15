using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        diginoteDB = new DiginoteDatabase();
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
        Log("[Server]: Added buy order from user " + newOrder.User);
        buyOrders.Add(newOrder);
    }

    public void AddSellOrder(Order newOrder)
    {
        Log("[Server]: Added sell order from user " + newOrder.User);
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

        Diginote dig;
        for (int i = 0; i < 4; i++) {
            dig = new Diginote();
            diginoteDB.AddDiginote(dig, newUser.Username);
        }


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

    public List<User> getLoggedUsers()
    {
        return loggedUsers;
    }

    public void Logout(User loguser)
    {
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Log("User logged out: " + loguser.Username);
    }

    public List<DiginoteInfo> DiginotesFromUser(User user)
    {
        List<DiginoteInfo> digs = new List<DiginoteInfo>();
        
        digs.Add(new DiginoteInfo(1, 1.0, DateTime.Now.ToString()));
        digs.Add(new DiginoteInfo(2, 1.0, DateTime.Now.ToString()));
        digs.Add(new DiginoteInfo(3, 1.0, DateTime.Now.ToString()));
        digs.Add(new DiginoteInfo(22, 1.2, DateTime.Now.ToString()));
        
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
            Pair<List<User>, DiginoteDatabase> state = (Pair<List<User>,DiginoteDatabase>)formatter.Deserialize(stream);
            usersList = state.first;
            diginoteDB = state.second;
            Log("State loaded.");
            stream.Close();
        }
        catch (Exception)
        {
            Log("Error loading state");
        }
    }

    private void saveState() 
    {
        try
        {
            Stream stream = File.Open(SAVE_FILENAME, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            Pair<List<User>, DiginoteDatabase> state = new Pair<List<User>, DiginoteDatabase>(usersList, diginoteDB);

            formatter.Serialize(stream, state);
            Log("State saved.");
            stream.Close();
        }
        catch (Exception)
        {
            Log("Error saving state.");
        }
    }

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Server]: Initialized Lifetime Service");
        return null;
    }
}