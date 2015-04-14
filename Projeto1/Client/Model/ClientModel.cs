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
            /*try
            {
                Quotation = diginoteSystem.GetQuotation();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Can't reach server");
                Console.WriteLine("ERROR: " + e.Message);
            }*/
        }

        // initialize variables
        private void Initialize()
        {
            // get diginote trading system interface instance
            diginoteSystem = (IDiginoteTradingSystem)RemoteNew.New(typeof(IDiginoteTradingSystem));

            evRepeater = new ChangeEventRepeater();

            user = null;
        }

        private void ChangeHandler(ChangeArgs args)
        {
            Console.WriteLine("Changer ocurred!!!");
        }

        public bool Login(string user, string password)
        {
            try
            {
                Pair<bool, User> result = diginoteSystem.Login(user, password);

                if (result.first)
                {
                    this.user = result.second;
                    evRepeater.ChangeEvent += new ChangeDelegate(ChangeHandler);
                    diginoteSystem.ChangeEvent += new ChangeDelegate(evRepeater.Repeater);
                    return true;
                }
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "DEFAULT");
            }

            return false;
        }

        public bool Register(string name, string user, string password)
        {
            try
            {
                if (diginoteSystem.RegisterUser(name, user, password))
                {
                    Login(user, password);
                    return true;
                }
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "DEFAULT");
            }

            return false;
        }

        // function that sets the lifetime service to infinite
        public override object InitializeLifetimeService() { return null; }
    }
}
