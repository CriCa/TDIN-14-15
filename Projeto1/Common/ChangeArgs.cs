using System;
using System.Collections;
using System.Collections.Generic;

public enum ChangeType { QuotationUp, QuotationDown, Transaction, Login, Logout, SysDiginotes } //other change types like ex.: buy order count 

// class that explains the change that occurred in server. add fields and constructors as needed
[Serializable]
public class ChangeArgs
{
    public ChangeType Type { get; set; }

    public double QuotationValue { get; set; }

    public string User1 { get; set; }

    public string User2 { get; set; }

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

    public ChangeArgs(ChangeType t, double quotationValue, string author)
    {
        Type = t;
        QuotationValue = quotationValue;
        User1 = null;
        User2 = author;
    }

    public ChangeArgs(string u1, string u2, List<DiginoteInfo> digs)
    {
        Type = ChangeType.Transaction;
        User1 = u1;
        User2 = u2;
        DiginotesTraded = digs;
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
}