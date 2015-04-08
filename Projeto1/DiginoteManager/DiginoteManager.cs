using System;
using System.Collections;

public class DiginoteManager : MarshalByRefObject, IDiginoteManager
{
    double quotation;
    ArrayList sellOrders;
    ArrayList buyOrders;

    public DiginoteManager()
    {
        quotation = 1.0;
        buyOrders = new ArrayList();
        sellOrders = new ArrayList();
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public double GetQuotation()
    {
        return quotation;
    }

    public void AddBuyOrder(Order newOrder)
    {
        buyOrders.Add(newOrder);
    }

    public void AddSellOrder(Order newOrder)
    {
        sellOrders.Add(newOrder);
    }
}
