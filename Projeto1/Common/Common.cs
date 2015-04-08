using System;

[Serializable]
public class UserEntity // user entity identifier
{
    public UserEntity(string name, string nick)
    {
        Name = name;
        Nick = nick;
    }

    public string Name { get; set; }

    public string Nick { get; set; }
}

/*
[Serializable]
public class Diginote
{
    static int nextSerial = 0;

    public Diginote() {
        Id = nextSerial++;
        Value = 1.0;
    }

    public Diginote(int id)
    {
        Id = id;
    }

    public int Id { get; set; }

    public double Value { get; set; }

    public static int NextSerial
    {
        get { return nextSerial; }
        set { nextSerial = value; }
    }
}*/

public enum OrderType { Buy, Sell }; // orders types

[Serializable]
public class Order
{
    public OrderType Type { get; set;  } // order type

    public int Quantity { get; set; } // quantity of diginotes to trade
    
    public string User { get; set;} // user that created the order

    public Order(OrderType t, int q, string u)
    {
        Type = t;
        Quantity = q;
        User = u;
    }
}

public enum QuotationChangeType { Up, Down };

public delegate void QuotationDelegate(QuotationChangeType type , double value);

public interface IDiginoteManager
{
    event QuotationDelegate QuotationChange;

    double GetQuotation(); // get current quotation

    void AddBuyOrder(Order newOrder); // add a buy order

    void AddSellOrder(Order newOrder); // add a sell order
}
