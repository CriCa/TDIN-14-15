using System;

public delegate void ChangeDelegate(ChangeArgs args);

public interface IDiginoteTradingSystem
{
    event ChangeDelegate ChangeEvent;

    double GetQuotation(); // get current quotation

    void SuggestNewQuotation(double value);

    void AddBuyOrder(Order newOrder); // add a buy order

    void AddSellOrder(Order newOrder); // add a sell order

    bool RegisterUser(string name, string username, string password); // register a new user

    Pair<bool, User> Login(string username, string password); // login user, return is equivalent to pair

    void Logout(User user);

    int DiginotesFromUser(User user);
}