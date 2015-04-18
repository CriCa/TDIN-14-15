using System;
using System.Collections.Generic;

public delegate void ChangeDelegate(ChangeArgs args);

public interface IDiginoteTradingSystem
{
    event ChangeDelegate ChangeEvent; // event to keep clients updated of changes in server

    // get current quotation
    double GetQuotation();

    // suggest new quotation by user
    void SuggestNewQuotation(User user, double value);

    // get general system info
    Tuple<int, int, int, int, int> GetSystemInfo();

    // function that adds a new buy order from a user and make transactions if possible
    Order AddBuyOrder(User user, int quantity, OrderType orderType);

    // function that adds a new sell order from a user and make transactions if possible
    Order AddSellOrder(User user, int quantity, OrderType orderType);

    // function that removes a order from the user
    Order RemoveOrder(User user, Order order);

    // function that handles the approval or disapproval of a user to keep an order after a quotation change
    Order ReceiveApproval(User user, Order order, bool appr);

    // function that regists a new user in the system return true in case of success and false otherwise
    bool RegisterUser(string name, string username, string password);

    // function that logins an user and returns an instance of 'User' class in case of success
    Pair<bool, User> Login(string username, string password);

    // function that logouts a user
    void Logout(User user);

    // function that returns a list with the diginotes info that are owned by an user
    List<DiginoteInfo> DiginotesFromUser(User user);

    // function that returns a list of the orders made by an user
    List<Order> OrdersFromUser(User user);

    // function that returns the time necessary to dig a new diginote
    double GetDigtime();

    // function that adds a new diginote to user and return info about the new diginote
    DiginoteInfo DigDiginote(User user);

    // getter for quotation statistics
    List<Pair<DateTime, double>> GetQuotationEvolution();

    // getter for transactions statistics
    List<Pair<DateTime, int>> GetTransactionsPerMin();
}