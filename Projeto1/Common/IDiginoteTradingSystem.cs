using System;

public enum QuotationChangeType { Up, Down };

public delegate void QuotationDelegate(QuotationChangeType type , double value);

public interface IDiginoteTradingSystem
{
    event QuotationDelegate QuotationChange;

    double GetQuotation(); // get current quotation

    void AddBuyOrder(Order newOrder); // add a buy order

    void AddSellOrder(Order newOrder); // add a sell order
}