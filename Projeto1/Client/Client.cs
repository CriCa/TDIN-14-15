using System;
using System.Collections;
using System.Runtime.Remoting;

class Client
{
    private int quotation; // current quotation
    ArrayList mySellOrders; // my sell orders
    ArrayList myBuyOrders; // my buy orders

    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Client.exe.config", false);
        IDiginoteTradingSystem diginoteSystem = (IDiginoteTradingSystem)RemoteNew.New(typeof(IDiginoteTradingSystem));

        diginoteSystem.AddBuyOrder(new Order(OrderType.Buy, 25, "user"));

        Console.ReadLine();
    }
}

class RemoteNew
{
    private static Hashtable types = null;

    private static void InitTypeTable()
    {
        types = new Hashtable();
        foreach (WellKnownClientTypeEntry entry in RemotingConfiguration.GetRegisteredWellKnownClientTypes())
            types.Add(entry.ObjectType, entry);
    }

    public static object New(Type type)
    {
        if (types == null)
            InitTypeTable();
        WellKnownClientTypeEntry entry = (WellKnownClientTypeEntry)types[type];
        if (entry == null)
            throw new RemotingException("Type not found!");
        return RemotingServices.Connect(type, entry.ObjectUrl);
    }
}
