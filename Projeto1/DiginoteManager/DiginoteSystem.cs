using System;
using System.Collections;

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