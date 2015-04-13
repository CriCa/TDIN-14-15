using System;
using System.Collections;

public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private double quotation; // current quotation of diginotes
    
    private ArrayList usersList;
    private ArrayList loggedUsers;

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

        // create logger
        logger = new Logger(ChangeEvent);

        diginoteDB = new DiginoteDatabase();
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


    public bool userExists(User newUser){
        foreach(User user in usersList){
            if (user.Username.Equals(newUser.Username))
                return true;
        }
        return false;
    }

    public bool registerUser(User newUser)
    {
        if (userExists(newUser))
            return false;

        usersList.Add(newUser);
        Console.WriteLine("[DiginoteSystem] user registered: {0}", newUser.Username);

       //SAVE STATE

        return true;
    }

    public bool checkLogin(User user)
    {
        return usersList.Contains(user);
    }

    public bool Login(User user) //DIFFERENT RETURNS NEEDED?
    {
        if (!checkLogin(user))
            return false;

        loggedUsers.Add(new User(user.Name, user.Username, user.Password));
        Console.WriteLine("[DiginoteSystem] user logged in: {0}", user.Username);

        return true;
    }

    public void Logout(User user)
    {
        loggedUsers.Remove(user);
        Console.WriteLine("[DiginoteSystem] user logged out: {0}", user.Username);
    }
}