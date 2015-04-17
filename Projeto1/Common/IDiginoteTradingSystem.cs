using System;
using System.Collections.Generic;

public delegate void ChangeDelegate(ChangeArgs args);

public interface IDiginoteTradingSystem
{
    event ChangeDelegate ChangeEvent;

    double GetQuotation(); // get current quotation

    void SuggestNewQuotation(User user, double value);

    Tuple<int, int, int, int, int> GetSystemInfo();

    Order AddBuyOrder(User user, int quantity, OrderType orderType); // add a buy order

    Order AddSellOrder(User user, int quantity, OrderType orderType); // add a sell order

    Order RemoveOrder(User user, Order order);

    Order ReceiveApproval(User user, Order order, bool appr);

    bool RegisterUser(string name, string username, string password); // register a new user

    Pair<bool, User> Login(string username, string password); // login user, return is equivalent to pair

    void Logout(User user);

    List<DiginoteInfo> DiginotesFromUser(User user);

    double GetDigtime();

    DiginoteInfo DigDiginote(User user);

    List<Order> OrdersFromUser(User user);

    List<Pair<DateTime, double>> GetQuotationEvolution();

    List<Pair<DateTime, int>> GetTransactionsPerMin();
}