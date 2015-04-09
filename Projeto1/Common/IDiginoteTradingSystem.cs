using System;

public delegate void ChangeDelegate(ChangeArgs args);

public interface IDiginoteTradingSystem
{
    event ChangeDelegate ChangeEvent;

    double GetQuotation(); // get current quotation

    void AddBuyOrder(Order newOrder); // add a buy order

    void AddSellOrder(Order newOrder); // add a sell order
}