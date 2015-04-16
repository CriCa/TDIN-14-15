using System;

public enum OrderType { Buy, Sell }; // orders types

public enum OrderState { Pending, Over, WaitApproval };

[Serializable]
public class Order
{
    public OrderType Type { get; set; } // order type

    public int Quantity { get; set; } // quantity of diginotes to trade

    public int InitialQuantity { get; set; } // initial quantity saved for history

    public User User { get; set; } // user that created the order

    public OrderState State { get; set; }

    public string Icon
    {
        get
        {
            if (Type == OrderType.Buy)
                return "../Resources/buy_icon.png";
            else
                return "../Resources/sell_icon.png";
        }
    }

    public Order(OrderType t, int q, User u)
    {
        Type = t;
        Quantity = q;
        InitialQuantity = q;
        User = u;
        State = OrderState.Pending;
    }
}