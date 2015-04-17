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

        public double DigTime { get; set; }

        public int NumUsers { get; set; }

        public int NumLoggedUsers { get; set; }

        public int NumSysDiginotes { get; set; }

        public int DiginotesOffer { get; set; }

        public int DiginotesDemand { get; set; }

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
                    App.Current.Dispatcher.Invoke((System.Action)(() =>
                    {
                        Quotation = args.QuotationValue;
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUOTATION, null), "");

                        if (Orders.Count > 0 && Orders[Orders.Count - 1].State == OrderState.Pending)
                        {
                            if (args.User2 != user.Username &&
                                ((args.Type == ChangeType.QuotationUp && Orders[Orders.Count - 1].Type == OrderType.Buy)
                                || (args.Type == ChangeType.QuotationDown && Orders[Orders.Count - 1].Type == OrderType.Sell)))
                            {
                                Order lastOrder = Orders[Orders.Count - 1];
                                Orders.Remove(lastOrder);
                                lastOrder.State = OrderState.WaitApproval;
                                Orders.Add(lastOrder);
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.MANTAINORDER, null), "");
                            }
                        }
                    }));
                }
                else if (args.Type == ChangeType.Transaction)
                {
                    GetDiginotes();
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.DIGINOTES, null), "");
                    GetOrders();
                   // UpdateOrders();
                   // GetOrders();
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ORDERS, null), "");
                }
                else if (args.Type == ChangeType.Login)
                {
                    NumUsers = args.NumUsers;
                    NumLoggedUsers = args.NumLoggedUsers;
                    NumSysDiginotes = args.NumSysDiginotes;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SYSTEMINFO, null), "");
                    
                }
                else if (args.Type == ChangeType.Logout)
                {
                    NumLoggedUsers = args.NumLoggedUsers;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SYSTEMINFO, null), "");
                }
                else if (args.Type == ChangeType.SysDiginotes)
                {
                    NumSysDiginotes = args.NumSysDiginotes;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SYSTEMINFO, null), "");
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

                    // get info
                    GetInfo();

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

                diginoteSystem.OrdersFromUser(user).ForEach(Orders.Add);

                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ORDERS, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void GetInfo()
        {
            try
            {
                Tuple<int, int, int, int, int> info = diginoteSystem.GetSystemInfo();

                NumUsers = info.Item1;
                NumLoggedUsers = info.Item2;
                NumSysDiginotes = info.Item3;
                DiginotesOffer = info.Item4;
                DiginotesDemand = info.Item5;

                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SYSTEMINFO, null), "");
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

        public void GetDigSeed()
        {
            try
            {
                DigTime = diginoteSystem.GetDigtime();
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void GetDiginoteDigged()
        {
            try
            {
                Diginotes.Add(diginoteSystem.DigDiginote(user));

                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.DIGINOTES, null), "");

                DigTime = diginoteSystem.GetDigtime();
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
                Orders.Add(diginoteSystem.AddBuyOrder(user, quantity, OrderType.Buy));
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NEWORDER, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NOSERVER, null), "");
            }
        }

        public void ChangeQuotationApproval(bool approve)
        {
            try
            {
                Order lastOrder = Orders[Orders.Count - 1];

                diginoteSystem.ReceiveApproval(user, approve, lastOrder.Type);
                GetOrders();
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ORDERS, null), "");
                /*
                if (!approve)
                { // shoudln't be done here to have the server date
                    Orders.Remove(lastOrder);
                    lastOrder.State = OrderState.Over;
                    Orders.Add(lastOrder);
                }
                else
                {
                    Orders.Remove(lastOrder);
                    lastOrder.State = OrderState.Pending;
                    Orders.Add(lastOrder);
                }*/
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
