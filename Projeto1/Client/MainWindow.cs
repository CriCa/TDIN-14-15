using System;
using System.Collections;
using System.Runtime.Remoting;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainWindow : Form
{
    private int quotation; // current quotation
    ArrayList mySellOrders; // my sell orders
    ArrayList myBuyOrders; // my buy orders
    IDiginoteTradingSystem diginoteSystem;

    public MainWindow()
    {
        RemotingConfiguration.Configure("Client.exe.config", false);
        InitializeComponent();

        diginoteSystem = (IDiginoteTradingSystem)RemoteNew.New(typeof(IDiginoteTradingSystem));

        setQuotation(diginoteSystem.GetQuotation());

        //test
        //diginoteSystem.AddBuyOrder(new Order(OrderType.Buy, 25, "user"));
    }

    private void setQuotation(double value) {
        label1.Text = "Quotation: " + value + " €";
    }
}

// Mechanism for instanciating a remote object through its interface, using the config file
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