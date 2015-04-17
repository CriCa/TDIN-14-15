using System;
using System.Collections;
using System.Collections.Generic;

public enum ChangeType { QuotationUp, QuotationDown, Transaction, Login, Logout, SysDiginotes, OfferDemand }

// class that explains the change that occurred in server
[Serializable]
public class ChangeArgs
{
    public ChangeType Type { get; set; }

    public double QuotationValue { get; set; }

    public Pair<DateTime, double> QuotationStat { get; set; }

    public Pair<DateTime, int> TransactionStat { get; set; }

    public string User1 { get; set; }

    public string User2 { get; set; }

    public Order Order1 { get; set; }

    public Order Order2 { get; set; }

    public List<DiginoteInfo> DiginotesTraded { get; set; }

    public int NumUsers { get; set; }

    public int NumLoggedUsers { get; set; }

    public int NumSysDiginotes { get; set; }

    public int DiginotesOffer { get; set; }

    public int DiginotesDemand { get; set; }

    public ChangeArgs(ChangeType t)
    {
        Type = t;
    }

    public ChangeArgs(ChangeType t, double quotationValue, string author, Pair<DateTime, double> s)
    {
        Type = t;
        QuotationValue = quotationValue;
        User1 = null;
        User2 = author;
        QuotationStat = s;
    }

    public ChangeArgs(Order o1, Order o2, List<DiginoteInfo> digs, Pair<DateTime, int> s)
    {
        Type = ChangeType.Transaction;
        User1 = o1.User.Username;
        User2 = o2.User.Username;
        Order1 = o1;
        Order2 = o2;
        DiginotesTraded = digs;
        TransactionStat = s;
    }

    public ChangeArgs(int nu, int nl, int nd)
    {
        Type = ChangeType.Login;
        User1 = User2 = null;
        NumUsers = nu;
        NumLoggedUsers = nl;
        NumSysDiginotes = nd;
    }

    public ChangeArgs(ChangeType t, int n)
    {
        Type = t;
        User1 = User2 = null;
        if (t == ChangeType.Logout)
            NumLoggedUsers = n;
        else if (t == ChangeType.SysDiginotes)
            NumSysDiginotes = n;
    }

    public ChangeArgs(int offer, int demand)
    {
        Type = ChangeType.OfferDemand;
        User1 = User2 = null;
        DiginotesOffer = offer;
        DiginotesDemand = demand;
    }
}