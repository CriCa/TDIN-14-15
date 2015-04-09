using System;
using System.Collections;

public enum ChangeType { QuotationUp, QuotationDown } //other change types like ex.: buy order count 

// class that explains the change that occurred in server. add fields and constructors as needed
[Serializable]
public class ChangeArgs
{
    public ChangeType Type { get; set; }

    public double QuotationValue { get; set; }

    public ChangeArgs(ChangeType t)
    {
        Type = t;
    }

    public ChangeArgs(ChangeType t, double quotationValue)
    {
        Type = t;
        QuotationValue = quotationValue;
    }
}