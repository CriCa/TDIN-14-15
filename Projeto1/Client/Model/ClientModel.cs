using System;
using System.Collections;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using System.Runtime.Remoting;
using Client.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Client.Model
{
    public class ClientModel : MarshalByRefObject
    {
        // fields
        private IDiginoteTradingSystem diginoteSystem;

        private ChangeEventRepeater evRepeater;

        public User user { get; set; }

        public double Quotation { get; set; } // current quotation

        public int DiginotesNumber { get { return Diginotes.Count; } } // number of diginotes that the user owns
        public ObservableCollection<DiginoteInfo> Diginotes { get; set; }

        // private ArrayList mySellOrders; // my sell orders
        // private ArrayList myBuyOrders; // my buy orders

        public ObservableCollection<Order> orders;


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

            Diginotes = new ObservableCollection<DiginoteInfo>();

            orders = new ObservableCollection<Order>();
            orders.Add(new Order(OrderType.Buy, 25, "usertempp"));
            orders.Add(new Order(OrderType.Sell, 2, "usertempp"));

        }

        private void ChangeHandler(ChangeArgs args)
        {
            if (args.Type == ChangeType.QuotationUp || args.Type == ChangeType.QuotationDown)
            {
                Quotation = diginoteSystem.GetQuotation();
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUOTATION, null), "");
            }
        }

        public bool Login(string user, string password)
        {
            try
            {
                Pair<bool, User> result = diginoteSystem.Login(user, password);

                if (result.first)
                {
                    this.user = result.second;
                    Quotation = diginoteSystem.GetQuotation();
                    GetDiginotesNumber();
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

        public void Logout()
        {
            diginoteSystem.Logout(user);
            user = null;
        }

        public void GetDiginotesNumber()
        {
            List<DiginoteInfo> digs = diginoteSystem.DiginotesFromUser(user);
            
            digs.ForEach(Diginotes.Add);

            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.DIGINOTES, null), "");
        }

        public void SetNewQuotation(double value)
        {
            diginoteSystem.SuggestNewQuotation(value);
        }

        public void Dig()
        {

        }

        public void SellDiginotes(int quantity)
        {

        }

        public void BuyDiginotes(int quantity)
        {

        }

        // function that sets the lifetime service to infinite
        public override object InitializeLifetimeService() { return null; }
    }
}
