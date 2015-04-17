using System;
using System.Collections;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using System.Runtime.Remoting;
using Client.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

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

        public ObservableCollection<DataPoint> QuotationEvolution { get; set; }

        public ObservableCollection<DataPoint> TransactionsPerMin { get; set; }

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

            QuotationEvolution = new ObservableCollection<DataPoint>();

            TransactionsPerMin = new ObservableCollection<DataPoint>();
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
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Quotation, null), "");

                        QuotationEvolution.Add(new DataPoint(DateTimeAxis.ToDouble(args.QuotationStat.first), args.QuotationStat.second));

                        if (Orders.Count > 0 && Orders[0].State == OrderState.Pending)
                        {
                            if (args.User2 != user.Username &&
                                ((args.Type == ChangeType.QuotationUp && Orders[0].Type == OrderType.Buy)
                                || (args.Type == ChangeType.QuotationDown && Orders[0].Type == OrderType.Sell)))
                            {
                                Order lastOrder = Orders[0];
                                Orders.Remove(lastOrder);
                                lastOrder.State = OrderState.WaitApproval;
                                Orders.Insert(0, lastOrder);
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskApprove, null), "");
                            }
                        }
                    }));
                }
                else if (args.Type == ChangeType.Transaction)
                {
                    App.Current.Dispatcher.Invoke((System.Action)(() =>
                    {
                        Order lastOrder = Orders[0];
                        Orders.Remove(lastOrder);

                        if (args.User1 == user.Username)
                        { // remove
                            Orders.Insert(0, args.Order1);
                            List<DiginoteInfo> toRemove = new List<DiginoteInfo>();
                            foreach (DiginoteInfo dInfo in Diginotes)
                                if (args.DiginotesTraded.Exists(d => d.Serial == dInfo.Serial))
                                    toRemove.Add(dInfo);
                            foreach (DiginoteInfo dInfo in toRemove)
                                Diginotes.Remove(dInfo);
                        }
                        else
                        { // add
                            Orders.Insert(0, args.Order2);
                            args.DiginotesTraded.ForEach(Diginotes.Add);
                        }

                        if (TransactionsPerMin.Count > 0)
                        {
                            DataPoint lastStatItem = TransactionsPerMin[TransactionsPerMin.Count - 1];
                            if (lastStatItem.X == DateTimeAxis.ToDouble(args.TransactionStat.first))
                            {
                                TransactionsPerMin.Remove(lastStatItem);
                                lastStatItem.Y++;
                                TransactionsPerMin.Add(lastStatItem);
                            }
                            else
                                TransactionsPerMin.Add(new DataPoint(DateTimeAxis.ToDouble(args.TransactionStat.first), args.TransactionStat.second));
                        }
                        else TransactionsPerMin.Add(new DataPoint(DateTimeAxis.ToDouble(args.TransactionStat.first), args.TransactionStat.second));

                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Transaction, null), "");
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Diginotes, null), "");
                        if (Orders[0].Quantity != 0)
                        {
                            if(Orders[0].Type == OrderType.Buy)
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "-" + Quotation);
                            else
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "+" + Quotation);
                        }
                            
                    }));
                }
                else if (args.Type == ChangeType.Login)
                {
                    NumUsers = args.NumUsers;
                    NumLoggedUsers = args.NumLoggedUsers;
                    NumSysDiginotes = args.NumSysDiginotes;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");

                }
                else if (args.Type == ChangeType.Logout)
                {
                    NumLoggedUsers = args.NumLoggedUsers;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");
                }
                else if (args.Type == ChangeType.SysDiginotes)
                {
                    NumSysDiginotes = args.NumSysDiginotes;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");
                }
                else if (args.Type == ChangeType.OfferDemand)
                {
                    DiginotesOffer = args.DiginotesOffer;
                    DiginotesDemand = args.DiginotesDemand;
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");
                }
            }
        }

        internal bool Login(string user, string password)
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

                    // get statistics
                    GetStatistics();

                    // subscribe changes
                    evRepeater.ChangeEvent += ChangeHandler;
                    diginoteSystem.ChangeEvent += evRepeater.Repeater;

                    // return success
                    return true;
                }
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "DEFAULT");
            }

            return false;
        }

        internal bool Register(string name, string user, string password)
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
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }

            return false;
        }

        internal void Logout()
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
                    QuotationEvolution = null;
                    TransactionsPerMin = null;
                }
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void GetDiginotes()
        {
            try
            {
                Diginotes = new ObservableCollection<DiginoteInfo>();

                diginoteSystem.DiginotesFromUser(user).ForEach(Diginotes.Add);
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void GetOrders()
        {
            try
            {
                Orders = new ObservableCollection<Order>();

                List<Order> reversed = diginoteSystem.OrdersFromUser(user);
                reversed.Reverse();

                reversed.ForEach(Orders.Add);
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void GetInfo()
        {
            try
            {
                Tuple<int, int, int, int, int> info = diginoteSystem.GetSystemInfo();

                NumUsers = info.Item1;
                NumLoggedUsers = info.Item2;
                NumSysDiginotes = info.Item3;
                DiginotesOffer = info.Item4;
                DiginotesDemand = info.Item5;
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void GetStatistics()
        {
            try
            {
                QuotationEvolution = new ObservableCollection<DataPoint>();
                TransactionsPerMin = new ObservableCollection<DataPoint>();

                foreach (Pair<DateTime, double> pair in diginoteSystem.GetQuotationEvolution())
                    QuotationEvolution.Add(new DataPoint(DateTimeAxis.ToDouble(pair.first), pair.second));

                foreach (Pair<DateTime, int> pair in diginoteSystem.GetTransactionsPerMin())
                    TransactionsPerMin.Add(new DataPoint(DateTimeAxis.ToDouble(pair.first), pair.second));
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void SetNewQuotation(double value)
        {
            try
            {
                diginoteSystem.SuggestNewQuotation(user, value);
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void GetDigSeed()
        {
            try
            {
                DigTime = diginoteSystem.GetDigtime();
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void GetDiginoteDigged()
        {
            try
            {
                if (user != null)
                {
                    Diginotes.Add(diginoteSystem.DigDiginote(user));

                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Diginotes, null), "");

                    DigTime = diginoteSystem.GetDigtime();
                }
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void SellDiginotes(int quantity)
        {
            try
            {
                Orders.Insert(0, diginoteSystem.AddSellOrder(user, quantity, OrderType.Sell));
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void BuyDiginotes(int quantity)
        {
            try
            {
                Orders.Insert(0, diginoteSystem.AddBuyOrder(user, quantity, OrderType.Buy));
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void RemoveOrder()
        {
            try
            {
                Order lastOrder = Orders[0];

                Order changedOrder = diginoteSystem.RemoveOrder(user, lastOrder);

                Orders.Remove(lastOrder);
                Orders.Insert(0, changedOrder);

                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        internal void ChangeQuotationApproval(bool approve)
        {
            try
            {
                Order lastOrder = Orders[0];

                Order changedOrder = diginoteSystem.ReceiveApproval(user, lastOrder, approve);

                Orders.Remove(lastOrder);
                Orders.Insert(0, changedOrder);
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that sets the lifetime service to infinite
        public override object InitializeLifetimeService() { return null; }
    }
}
