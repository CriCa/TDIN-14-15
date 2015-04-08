using System;
using System.IO;
using System.Collections;

public class DiginoteManager : MarshalByRefObject, IDiginoteManager
{
    private double quotation; // current quotation of diginotes
    
    private ArrayList sellOrders; // list of sell orders
    private ArrayList buyOrders; // list of buy orders

    private Logger logger; // log system

    public event QuotationDelegate QuotationChange; // event to warn clients 
                                                    // when quotation changes

    public DiginoteManager()
    {
        // TODO check for log and load state

        // set initial quotation
        quotation = 1.0;

        // create order lists
        buyOrders = new ArrayList();
        sellOrders = new ArrayList();

        // create logger
        logger = new Logger(this);
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
            QuotationChangeType type;
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
public class Logger 
{
    public const string LOG_FILENAME = "log.txt";
    private StreamWriter file;

    public Logger(DiginoteManager ds)
    {
        // creating the file WARNING: for now this is creating a new file
        // later on we need to change to append mode
        file = new StreamWriter(LOG_FILENAME);
        Log(DateTime.Now + "|Created log file");

        // subscribe events
        ds.QuotationChange += QuotationChangeHandler;
    }

    public void Log(string msg)
    {
        // write msg to file
        file.WriteLine(msg);
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