using System;

public enum OrderType { Buy, Sell }; // orders types

public enum OrderState { Pending, WaitApproval, Over, Removed }; // order states

/**
 * class that represents and describes an order
 */
[Serializable]
public class Order
{
    public OrderType Type { get; set; } // order type

    int quantity; // // quantity of diginotes to trade

    public int Quantity
    { 
        get { return quantity; }
        set { quantity = value; if (quantity == 0) State = OrderState.Over; } // if quantity reachs 0 then close order
    } 

    public int InitialQuantity { get; set; } // initial quantity saved for history

    public User User { get; set; } // user that created the order

    private OrderState state; // order state

    public OrderState State // state of the order
    {
        get { return state; }
        set
        {
            state = value;
            if (state == OrderState.Over || state == OrderState.Removed)
                FinishedOn = DateTime.Now.ToString();
        }
    }

    public string CreatedOn { get; set; } // creation time

    public string FinishedOn { get; set; } // time when the order was closed

    public string Icon // icon in client interface
    {
        get
        {
            if (Type == OrderType.Buy)
                return "../Resources/buy_icon.png";
            else
                return "../Resources/sell_icon.png";
        }
    }

    public string TypeDesc // type description in client interface
    {
        get
        {
            if (Type == OrderType.Buy)
                return "Buy";
            else return "Sell";
        }
    }

    public string StateDesc // state description in client interface
    {
        get
        {
            if (State == OrderState.Pending)
                return "Pending";
            else if (State == OrderState.Over)
                return "Over";
            else if (State == OrderState.WaitApproval)
                return "Wait Approval";
            else return "Removed";
        }
    }

    public string StateColor // color in client interface
    {
        get
        {
            if (State == OrderState.Pending || State == OrderState.WaitApproval)
                return "#1900FF00";
            else if (State == OrderState.Over)
                return "#19000000";
            else return "#19FF0000";
        }
    }

    // constructor
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