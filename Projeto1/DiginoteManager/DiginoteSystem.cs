using System;
using System.IO;
using System.Collections;

// diginote class
public class Diginote
{
    static int nextSerial = 0;

    public Diginote()
    {
        Id = nextSerial++;
        Value = 1.0;
    }

    public Diginote(int id)
    {
        Id = id;
    }

    public int Id { get; set; }

    public double Value { get; set; }

    public static int NextSerial
    {
        get { return nextSerial; }
        set { nextSerial = value; }
    }
}

class DiginoteDatabase
{
    private Hashtable diginotesOwners;

    public DiginoteDatabase()
    {
        diginotesOwners = new Hashtable();
    }

    public void AddDiginote(Diginote dig, string user)
    {
        diginotesOwners.Add(dig, user);
    }
}

public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private double quotation; // current quotation of diginotes
    
    private ArrayList sellOrders; // list of sell orders
    private ArrayList buyOrders; // list of buy orders

    private Logger logger; // log system

    public event QuotationDelegate QuotationChange; // event to warn clients 
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
        logger = new Logger(this);

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

    private void setNewQuotation(double value)
    {
        if(value > quotation) {
            Console.WriteLine("[Server]: Quotation value went up to: " + value);
            QuotationChange(QuotationChangeType.Up, value);
        }
        else {
            Console.WriteLine("[Server]: Quotation value went down to: " + value);
            QuotationChange(QuotationChangeType.Down, value);
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
}

// class that creates the log file writes system changes
class Logger 
{
    public const string LOG_FILENAME = "log.txt";
    private StreamWriter file;

    public Logger(DiginoteTradingSystem ds)
    {
        // creating the file WARNING: for now this is creating a new file
        // later on we need to change to append mode
        file = new StreamWriter(LOG_FILENAME);
        Log("Created log file");

        // subscribe events
        ds.QuotationChange += QuotationChangeHandler;
    }

    public void Log(string msg)
    {
        // write msg to file
        file.WriteLine(DateTime.Now + "|" + msg);
        file.Flush();

        // show on console
        Console.WriteLine("[Logger]: " + msg);
    }

    public void QuotationChangeHandler(QuotationChangeType type, double value)
    {
        // log the new quotation value
        if (type == QuotationChangeType.Up)
            Log("[Server]: Quotation value went up to: " + value);
        else
            Log("[Server]: Quotation value went down to: " + value);
    }
}