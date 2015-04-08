using System;
using System.Collections;
using System.Runtime.Remoting;

class Client
{
    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Client.exe.config", false);
        IDiginoteManager diginoteSystem = (IDiginoteManager)RemoteNew.New(typeof(IDiginoteManager));

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
