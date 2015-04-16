using System;

public enum OrderType { Buy, Sell }; // orders types

public enum OrderState { Pending, WaitApproval, Over, Removed };

[Serializable]
public class Order
{
    private OrderState state;

    public OrderType Type { get; set; } // order type

    public string TypeDesc
    {
        get
        {
            if (Type == OrderType.Buy)
                return "Buy";
            else return "Sell";
        }
    }

    public int Quantity { get; set; } // quantity of diginotes to trade

    public int InitialQuantity { get; set; } // initial quantity saved for history

    public User User { get; set; } // user that created the order

    public OrderState State
    {
        get { return state; }
        set
        {
            state = value;
            if (state == OrderState.Over || state == OrderState.Removed)
                FinishedOn = DateTime.Now.ToString();
        }
    }

    public string CreatedOn { get; set; }

    public string FinishedOn { get; set; }

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

    public string StateDesc
    {
        get
        {
            if (State == OrderState.Pending)
                return "Pending";
            else if (State == OrderState.Over)
                return "Over";
            else if (State == OrderState.WaitApproval)
                return "Wait Approval";
            else return "Error";
        }
    }

    public Order(OrderType t, int q, User u)
    {
        Type = t;
        Quantity = InitialQuantity = q;
        User = u;
        State = OrderState.Pending;
        CreatedOn = DateTime.Now.ToString();
        FinishedOn = null;
    }
}