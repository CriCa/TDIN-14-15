using System;

public enum OrderType { Buy, Sell }; // orders types

[Serializable]
public class Order
{
    public OrderType Type { get; set; } // order type

    public int Quantity { get; set; } // quantity of diginotes to trade

    public string User { get; set; } // user that created the order

    public Order(OrderType t, int q, string u)
    {
        Type = t;
        Quantity = q;
        User = u;
    }
}