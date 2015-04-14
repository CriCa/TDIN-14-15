using System;
using System.Collections;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using System.Runtime.Remoting;
using Client.Helpers;

namespace Client.Model
{
    public class ClientModel : MarshalByRefObject
    {
        // fields
        private IDiginoteTradingSystem diginoteSystem;

        private ChangeEventRepeater evRepeater;

        private User user;

        private double Quotation { get; set; } // current quotation

        // private ArrayList mySellOrders; // my sell orders
        // private ArrayList myBuyOrders; // my buy orders
        
        

        // constructor
        public ClientModel()
        {
            // set configuration
            RemotingConfiguration.Configure("Client.exe.config", false);

            // initialize
            Initialize();

            // tests
            try
            {
                Quotation = diginoteSystem.GetQuotation();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Can't reach server");
                Console.WriteLine("ERROR: " + e.Message);
            }
        }

        // initialize variables
        private void Initialize()
        {
            // get diginote trading system interface instance
            diginoteSystem = (IDiginoteTradingSystem)RemoteNew.New(typeof(IDiginoteTradingSystem));

            evRepeater = new ChangeEventRepeater();

            user = null;

            // this should be done only after login
            evRepeater.ChangeEvent += new ChangeDelegate(ChangeHandler);
            diginoteSystem.ChangeEvent += new ChangeDelegate(evRepeater.Repeater);
        }

        private void ChangeHandler(ChangeArgs args)
        {
            Console.WriteLine("Changer ocurred!!!");
        }

        public bool Login(string user, string password)
        {
            Console.WriteLine("Loging in: " + user + "-" + password);
            return true;
        }

        public bool Register(string name, string user, string password)
        {
            Console.WriteLine("Registering: " + name + "-" + user + "-" + password);
            return true;
        }

        // function that sets the lifetime service to infinite
        public override object InitializeLifetimeService() { return null; }
    }
}
