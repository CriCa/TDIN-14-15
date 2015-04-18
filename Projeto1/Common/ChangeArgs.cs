using System;
using System.Collections;
using System.Collections.Generic;

public enum ChangeType { QuotationUp, QuotationDown, Transaction, Login, Logout, SysDiginotes, OfferDemand } // change type

/**
 * Class that describes the changes that occurred in server so the client know how to update
 */
[Serializable]
public class ChangeArgs
{
    public ChangeType Type { get; set; } // change type

    public double QuotationValue { get; set; } // current quotation value

    public string User1 { get; set; } // User 1 involved

    public string User2 { get; set; } // User 2 involved

    public Order Order1 { get; set; } // Order from user 1

    public Order Order2 { get; set; } // Order from user 2

    public List<DiginoteInfo> DiginotesTraded { get; set; } // list of diginotes traded

    public int NumUsers { get; set; } // number of users in system

    public int NumLoggedUsers { get; set; } // number of logged users in system

    public int NumSysDiginotes { get; set; } // number of diginotes in system

    public int DiginotesOffer { get; set; } // number of diginotes selling

    public int DiginotesDemand { get; set; } // number of diginotes buying

    public Pair<DateTime, double> QuotationStat { get; set; } // quotation stat

    public Pair<DateTime, int> TransactionStat { get; set; } // transaction stat

    // constructor for quotation change
    public ChangeArgs(ChangeType t, double quotationValue, string author, Pair<DateTime, double> stat)
    {
        Type = t;
        QuotationValue = quotationValue;
        User1 = null;
        User2 = author;
        QuotationStat = stat;
    }

    // constructor for transaction
    public ChangeArgs(Order orderFrom, Order orderTo, List<DiginoteInfo> digs, Pair<DateTime, int> stat)
    {
        Type = ChangeType.Transaction;
        User1 = orderFrom.User.Username;
        User2 = orderTo.User.Username;
        Order1 = orderFrom;
        Order2 = orderTo;
        DiginotesTraded = digs;
        TransactionStat = stat;
    }

    // constructor for login
    public ChangeArgs(int numUsers, int numLogged, int numDiginotes)
    {
        Type = ChangeType.Login;
        User1 = User2 = null;
        NumUsers = numUsers;
        NumLoggedUsers = numLogged;
        NumSysDiginotes = numDiginotes;
    }

    // constructor for logout
    public ChangeArgs(ChangeType t, int numLogged)
    {
        Type = t;
        User1 = User2 = null;
        if (t == ChangeType.Logout)
            NumLoggedUsers = numLogged;
        else if (t == ChangeType.SysDiginotes)
            NumSysDiginotes = numLogged;
    }

    // constructor for removal
    public ChangeArgs(int numOffer, int numDemand)
    {
        Type = ChangeType.OfferDemand;
        User1 = User2 = null;
        DiginotesOffer = numOffer;
        DiginotesDemand = numDemand;
    }
}