using System;
using System.Collections.Generic;

public delegate void ChangeDelegate(ChangeArgs args);

public interface IDiginoteTradingSystem
{
    event ChangeDelegate ChangeEvent;

    double GetQuotation(); // get current quotation

    void SuggestNewQuotation(double value);

    void AddBuyOrder(Order newOrder); // add a buy order

    Order AddSellOrder(Order newOrder); // add a sell order

    bool RegisterUser(string name, string username, string password); // register a new user

    Pair<bool, User> Login(string username, string password); // login user, return is equivalent to pair

    void Logout(User user);

    List<DiginoteInfo> DiginotesFromUser(User user);


}