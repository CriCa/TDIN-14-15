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

        public ObservableCollection<Order> Orders { get; set; }

        // constructor
        public ClientModel()
        {
            // set configuration
            RemotingConfiguration.Configure("Client.exe.config", false);

            // initialize
            Initialize();
        }

        // initialize variables
        private void Initialize()
        {
            // get diginote trading system interface instance
            diginoteSystem = (IDiginoteTradingSystem)RemoteNew.New(typeof(IDiginoteTradingSystem));

            evRepeater = new ChangeEventRepeater();

            user = null;

            Quotation = 0.0;

            Diginotes = new ObservableCollection<DiginoteInfo>();

            Orders = new ObservableCollection<Order>();
        }

        private void ChangeHandler(ChangeArgs args)
        {
            if (args.User1 == null || args.User1 == user.Username || args.User2 == user.Username)
            {
                if (args.Type == ChangeType.QuotationUp || args.Type == ChangeType.QuotationDown)
                {
                    Quotation = args.QuotationValue;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUOTATION, null), "");
                }
                else if (args.Type == ChangeType.Transaction)
                {

                }
            }
        }

        public bool Login(string user, string password)
        {
            try
            {
                Pair<bool, User> result = diginoteSystem.Login(user, password);

                if (result.first)
                {
                    // set user
                    this.user = result.second;

                    // get actual quotation
                    Quotation = diginoteSystem.GetQuotation();

                    // get diginotes
                    GetDiginotes();

                    // get orders
                    GetOrders();

                    // subscribe changes
                    evRepeater.ChangeEvent += ChangeHandler;
                    diginoteSystem.ChangeEvent += evRepeater.Repeater;

                    // return success
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
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }

            return false;
        }

        public void Logout()
        {
            try
            {
                if (this.user != null)
                {
                    diginoteSystem.Logout(user);

                    evRepeater.ChangeEvent -= ChangeHandler;
                    diginoteSystem.ChangeEvent -= evRepeater.Repeater;

                    user = null;
                    Quotation = 0.0;
                    Diginotes = null;
                    Orders = null;
                }
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void GetDiginotes()
        {
            try
            {
                Diginotes = new ObservableCollection<DiginoteInfo>();

                diginoteSystem.DiginotesFromUser(user).ForEach(Diginotes.Add);

                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.DIGINOTES, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void GetOrders()
        {
            try
            {
                Orders = new ObservableCollection<Order>();
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }


        public void SetNewQuotation(double value)
        {
            try
            {
                diginoteSystem.SuggestNewQuotation(user, value);
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void Dig()
        {
            try
            {

            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void SellDiginotes(int quantity)
        {
            try
            {
                Orders.Add(diginoteSystem.AddSellOrder(user, quantity, OrderType.Sell));
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NEWORDER, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void BuyDiginotes(int quantity)
        {
            try
            {
                Orders.Add(diginoteSystem.AddSellOrder(user, quantity, OrderType.Buy));
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NEWORDER, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        // function that sets the lifetime service to infinite
        public override object InitializeLifetimeService() { return null; }
    }
}
