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
        private IDiginoteTradingSystem diginoteSystem; // diginote system interface instance

        private ChangeEventRepeater evRepeater; // event that receives the updates from server

        public User user { get; set; } // user logged

        public double Quotation { get; set; } // current quotation

        public int DiginotesNumber { get { return Diginotes.Count; } } // number of diginotes that the user owns

        public ObservableCollection<DiginoteInfo> Diginotes { get; set; } // info of the diginotes that the user owns

        public ObservableCollection<Order> Orders { get; set; } // orders made by user

        public double DigTime { get; set; } // time to dig a new diginote

        public int NumUsers { get; set; } // number of users in the system

        public int NumLoggedUsers { get; set; } // number of logged users in the system

        public int NumSysDiginotes { get; set; } // number of diginotes in the system

        public int DiginotesOffer { get; set; } // number of diginotes that can be bought

        public int DiginotesDemand { get; set; } // number of diginotes that can be selled

        public ObservableCollection<DataPoint> QuotationEvolution { get; set; } // quotation statistics

        public ObservableCollection<DataPoint> TransactionsPerMin { get; set; } // transactions statistics

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

            // create event repeater instance
            evRepeater = new ChangeEventRepeater();

            // initialize data
            user = null;

            Quotation = 0.0;

            Diginotes = new ObservableCollection<DiginoteInfo>();

            Orders = new ObservableCollection<Order>();

            QuotationEvolution = new ObservableCollection<DataPoint>();

            TransactionsPerMin = new ObservableCollection<DataPoint>();
        }

        // handler for incoming messenger notifications
        private void ChangeHandler(ChangeArgs args)
        {
            if (args.User1 == null || args.User1 == user.Username || args.User2 == user.Username) // if the logged user can receive the message
            {
                if (args.Type == ChangeType.QuotationUp || args.Type == ChangeType.QuotationDown) // quotation changed
                {
                    App.Current.Dispatcher.Invoke((System.Action)(() =>
                    {
                        // set new quotation value
                        Quotation = args.QuotationValue;

                        // notify change
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Quotation, null), "");

                        // add new stat
                        QuotationEvolution.Add(new DataPoint(DateTimeAxis.ToDouble(args.QuotationStat.first), args.QuotationStat.second));

                        if (Orders.Count > 0 && Orders[0].State == OrderState.Pending) // if last order is pending
                        {
                            if (args.User2 != user.Username &&
                                ((args.Type == ChangeType.QuotationUp && Orders[0].Type == OrderType.Buy)
                                || (args.Type == ChangeType.QuotationDown && Orders[0].Type == OrderType.Sell)))
                                // if the new quotation needs to be appproved by the user
                            {
                                // get last order
                                Order lastOrder = Orders[0];
                                
                                // remove and set new state
                                Orders.Remove(lastOrder);
                                lastOrder.State = OrderState.WaitApproval;

                                // add to the list, we need to remove and add to cast the listview update
                                Orders.Insert(0, lastOrder);

                                // notify main window to show approve dialog
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskApprove, null), "");
                            }
                        }
                    }));
                }
                else if (args.Type == ChangeType.Transaction) // transaction occurred
                {
                    App.Current.Dispatcher.Invoke((System.Action)(() =>
                    {
                        // get last order
                        Order lastOrder = Orders[0];

                        // remove order
                        Orders.Remove(lastOrder);

                        if (args.User1 == user.Username) // if the user logged sold the diginotes
                        {
                            // insert the changed order, we need to remove and add to cast the listview update
                            Orders.Insert(0, args.Order1);

                            // look for and remove the diginotes traded
                            List<DiginoteInfo> toRemove = new List<DiginoteInfo>();
                            foreach (DiginoteInfo dInfo in Diginotes)
                                if (args.DiginotesTraded.Exists(d => d.Serial == dInfo.Serial))
                                    toRemove.Add(dInfo);
                            foreach (DiginoteInfo dInfo in toRemove)
                                Diginotes.Remove(dInfo);
                        }
                        else if (args.User2 == user.Username) // if the user logged bought the diginotes
                        {
                            // insert the changed order, we need to remove and add to cast the listview update
                            Orders.Insert(0, args.Order2);

                            // add the diginotes traded
                            args.DiginotesTraded.ForEach(Diginotes.Add);
                        }

                        // update transaction statistics
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

                        // notify changes
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Transaction, null), "");
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Diginotes, null), "");

                        // if the order of the logged user wasn't totally satifiest, notify main window to show the dialog to ask for a new quotation
                        if (Orders[0].Quantity != 0)
                        {
                            if(Orders[0].Type == OrderType.Buy)
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "+" + Quotation);
                            else
                                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "-" + Quotation);
                        }
                    }));
                }
                else if (args.Type == ChangeType.Login) // login occurred
                {
                    // set data
                    NumUsers = args.NumUsers;
                    NumLoggedUsers = args.NumLoggedUsers;
                    NumSysDiginotes = args.NumSysDiginotes;

                    // notify update
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");

                }
                else if (args.Type == ChangeType.Logout) // logout occurred
                {
                    // set data
                    NumLoggedUsers = args.NumLoggedUsers;

                    // notify update
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");
                }
                else if (args.Type == ChangeType.SysDiginotes) // update on number of diginotes in system
                {
                    // set data
                    NumSysDiginotes = args.NumSysDiginotes;

                    // notify update
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");
                }
                else if (args.Type == ChangeType.OfferDemand) // update in number of diginotes to buy or sell
                {
                    // set data
                    DiginotesOffer = args.DiginotesOffer;
                    DiginotesDemand = args.DiginotesDemand;

                    // notify update
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SystemInfo, null), "");
                }
            }
        }

        // function that performs the login on server
        internal bool Login(string user, string password)
        {
            try
            {
                Pair<bool, User> result = diginoteSystem.Login(user, password); // try login in server

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
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "DEFAULT");
            }

            return false;
        }

        // function that performs the registration on server
        internal bool Register(string name, string user, string password)
        {
            try
            {
                if (diginoteSystem.RegisterUser(name, user, password)) // try register in server
                {
                    // if success then login
                    Login(user, password);
                    return true;
                }
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }

            return false;
        }

        // function that performs the logout on server
        internal void Logout()
        {
            try
            {
                if (this.user != null) // if not already logged out
                {
                    // logout in server
                    diginoteSystem.Logout(user);

                    // unsubscribe changes
                    evRepeater.ChangeEvent -= ChangeHandler;
                    diginoteSystem.ChangeEvent -= evRepeater.Repeater;

                    // reset data
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
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that gets the information about diginotes owned by the logged user in the server
        internal void GetDiginotes()
        {
            try
            {
                // initialize list
                Diginotes = new ObservableCollection<DiginoteInfo>();

                // get diginotes info and adds them to the list
                diginoteSystem.DiginotesFromUser(user).ForEach(Diginotes.Add);
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that gets orders made by the logged user in the server
        internal void GetOrders()
        {
            try
            {
                // initialize list
                Orders = new ObservableCollection<Order>();

                // get the orders
                List<Order> reversed = diginoteSystem.OrdersFromUser(user);

                // reverse
                reversed.Reverse();

                // adds them to the list
                reversed.ForEach(Orders.Add);
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that gets the information about system in the server
        internal void GetInfo()
        {
            try
            {
                // get info
                Tuple<int, int, int, int, int> info = diginoteSystem.GetSystemInfo();

                // set data
                NumUsers = info.Item1;
                NumLoggedUsers = info.Item2;
                NumSysDiginotes = info.Item3;
                DiginotesOffer = info.Item4;
                DiginotesDemand = info.Item5;
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that gets the statistics information in the server
        internal void GetStatistics()
        {
            try
            {
                // initialize lists
                QuotationEvolution = new ObservableCollection<DataPoint>();
                TransactionsPerMin = new ObservableCollection<DataPoint>();


                // add stats to the list
                foreach (Pair<DateTime, double> pair in diginoteSystem.GetQuotationEvolution())
                    QuotationEvolution.Add(new DataPoint(DateTimeAxis.ToDouble(pair.first), pair.second));

                // add stats to the list
                foreach (Pair<DateTime, int> pair in diginoteSystem.GetTransactionsPerMin())
                    TransactionsPerMin.Add(new DataPoint(DateTimeAxis.ToDouble(pair.first), pair.second));
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that suggests the new quotation in the server
        internal void SetNewQuotation(double value)
        {
            try
            {
                // suggest quotation
                diginoteSystem.SuggestNewQuotation(user, value);
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that add a new sell order in server
        internal void SellDiginotes(int quantity)
        {
            try
            {
                // add sell order in server and add the returned order to orders list
                Orders.Insert(0, diginoteSystem.AddSellOrder(user, quantity, OrderType.Sell));

                // notify udpdate
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that add a new buy order in server
        internal void BuyDiginotes(int quantity)
        {
            try
            {
                // add sell order in server and add the returned order to orders list
                Orders.Insert(0, diginoteSystem.AddBuyOrder(user, quantity, OrderType.Buy));

                // notify udpdate
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that removes the current active order from user in the server
        internal void RemoveOrder()
        {
            try
            {
                // get last order
                Order lastOrder = Orders[0];

                // remove order in server
                Order changedOrder = diginoteSystem.RemoveOrder(user, lastOrder);

                // update list with the changed order, we need to remove and add to cast the listview update
                Orders.Remove(lastOrder);
                Orders.Insert(0, changedOrder);

                // notify update
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that sends the approval or disapproval of keeping an order active to the server
        internal void ChangeQuotationApproval(bool approve)
        {
            try
            {
                // get last order
                Order lastOrder = Orders[0];

                // send approval to the server
                Order changedOrder = diginoteSystem.ReceiveApproval(user, lastOrder, approve);

                // update list with the changed order, we need to remove and add to cast the listview update
                Orders.Remove(lastOrder);
                Orders.Insert(0, changedOrder);

                // notify update
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Order, null), "");
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that gets the time to dig a new diginote
        internal void GetDigSeed()
        {
            try
            {
                // get time
                DigTime = diginoteSystem.GetDigtime();
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // function that gets the new diginote digged and the new time needed to dig another diginote
        internal void GetDiginoteDigged()
        {
            try
            {
                if (user != null) // if not already logged out
                {
                    // add new diginote
                    Diginotes.Add(diginoteSystem.DigDiginote(user));

                    // notify update because we have a new diginote
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Diginotes, null), "");

                    // get new time needed to dig
                    DigTime = diginoteSystem.GetDigtime();
                }
            }
            catch
            {
                // couldn't reach server
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.NoServer, null), "");
            }
        }

        // overiding lifetime to infinite
        public override object InitializeLifetimeService() { return null; }
    }
}
