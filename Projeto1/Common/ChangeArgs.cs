using System;
using System.Collections;
using System.Collections.Generic;

public enum ChangeType { QuotationUp, QuotationDown, Transaction } //other change types like ex.: buy order count 

// class that explains the change that occurred in server. add fields and constructors as needed
[Serializable]
public class ChangeArgs
{
    public ChangeType Type { get; set; }

    public double QuotationValue { get; set; }

    public string User1 { get; set; }

    public string User2 { get; set; }

    public List<DiginoteInfo> DiginotesTraded { get; set; }

    public ChangeArgs(ChangeType t)
    {
        Type = t;
    }

    public ChangeArgs(ChangeType t, double quotationValue)
    {
        Type = t;
        QuotationValue = quotationValue;
        User1 = User2 = null;
    }

    public ChangeArgs(string u1, string u2, List<DiginoteInfo> digs)
    {
        Type = ChangeType.Transaction;
        User1 = u1;
        User2 = u2;
        DiginotesTraded = digs;
    }
}