using System;
using Client.Helpers;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows.Threading;

namespace Client.ViewModel
{
    class MainViewModel : BaseViewModel
    {
        private ObservableCollection<DiginoteInfo> diginotes; // diginotes of the logged user

        private ObservableCollection<Order> orders; // orders made by the logged user

        private bool canSell; // can sell diginotes
        private bool canBuy; // can buy diginotes

        private bool canLower; // can lower the quotation
        private bool canRise; // can rise the quotation

        private bool canRemove; // can remove an order

        private bool digging; // is digging diginotes
        private double digStep; // dig step
        private int digTime; // time needed to dig a diginote
        private DispatcherTimer timer; // dig timer

        public ICommand LogoutCommand { get { return new RelayCommand(Logout); } } // logout command

        public ICommand SellCommand { get { return new RelayCommand(Sell); } } // sell command

        public ICommand BuyCommand { get { return new RelayCommand(Buy); } } // buy command

        public ICommand ChangeQuotationCommand { get { return new RelayCommand(ChangeQuotation); } } // change quotation command

        public ICommand CloseCommand { get { return new RelayCommand(Close); } } // close command

        public ICommand RemoveCommand { get { return new RelayCommand(RemoveOrder); } } // remove order command

        public ICommand DigCommand { get { return new RelayCommand(Dig); } } // dig command

        public string Username { get { return client.user.Name; } } // name of the logged user

        public double Quotation { get { return client.Quotation; } } // quotation

        public double DiginotesNumber { get { return client.Diginotes.Count; } } // number of diginotes

        public string DigButtonText { get { if (digging) return "Stop"; else return "Start"; } } // dig button text

        public int NumUsers { get { return client.NumUsers; } } // number of users in system

        public int NumLoggedUsers { get { return client.NumLoggedUsers; } } // number of users logged

        public int NumSysDiginotes { get { return client.NumSysDiginotes; } } // number of diginotes in system

        public int DiginotesOffer { get { return client.DiginotesOffer; } } // number of diginotes available to buy

        public int DiginotesDemand { get { return client.DiginotesDemand; } } // number of diginotes available to sell

        public ObservableCollection<Order> Orders { get { return orders; } } // orders made by the user

        public ObservableCollection<DiginoteInfo> Diginotes { get { return diginotes; } } // diginotes owned by the user

        public ObservableCollection<DataPoint> QuotationEvolution { get { return client.QuotationEvolution; } } // quotation statistics

        public ObservableCollection<DataPoint> TransactionsPerMin { get { return client.TransactionsPerMin; } } // transactions statistics

        public bool CanSell
        {
            get { return canSell; }
            set { canSell = value; RaisePropertyChangedEvent("CanSell"); }
        }

        public bool CanBuy
        {
            get { return canBuy; }
            set { canBuy = value; RaisePropertyChangedEvent("CanBuy"); }
        }

        public bool CanLower
        {
            get { return canLower; }
            set { canLower = value; RaisePropertyChangedEvent("CanLower"); }
        }

        public bool CanRise
        {
            get { return canRise; }
            set { canRise = value; RaisePropertyChangedEvent("CanRise"); }
        }

        public bool CanRemove
        {
            get { return canRemove; }
            set { canRemove = value; RaisePropertyChangedEvent("CanRemove"); }
        }

        public double DigStep
        {
            get { return digStep; }
            set { digStep = value; RaisePropertyChangedEvent("DigStep"); }
        }

        // constructor
        public MainViewModel()
        {
            // subscribe messenger
            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);

            // set diginotes and orders lists
            diginotes = client.Diginotes;
            orders = client.Orders;

            // set active buttons
            SetActiveButtons();

            // set digging
            digging = false;

            if (Orders.Count > 0 && Orders[0].State == OrderState.WaitApproval) {  // if the last order is waiting for approval
                // create new timer
                DispatcherTimer timer = new DispatcherTimer();

                // start a timer to ask approval after 1 second
                timer.Interval = TimeSpan.FromSeconds(1d);
                timer.Tick += QueryApproval;
                timer.Start();
            }
        }

        // handler for incoming messenger notifications
        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.NoServer) // can´t reach server
            {
                System.Windows.MessageBox.Show(Parent, "Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(-1);
            }
            else if (msg.Content.Type == NotifType.LogOut) // logout
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
            else if (msg.Content.Type == NotifType.Quotation) // quotation changed
                RaisePropertyChangedEvent("Quotation");
            else if (msg.Content.Type == NotifType.Diginotes) // diginotes changed
                RaisePropertyChangedEvent("DiginotesNumber");
            else if (msg.Content.Type == NotifType.SetQuotation) // user entered a new quotation
                client.SetNewQuotation(Double.Parse((string)msg.Notification));
            else if (msg.Content.Type == NotifType.Order || msg.Content.Type == NotifType.Transaction) // new order or transaction
                SetActiveButtons();
            else if (msg.Content.Type == NotifType.ApproveQuotation) // user approved or disapproved keeping the order
            {
                if (msg.Notification == "Approve")
                    client.ChangeQuotationApproval(true);
                else
                    client.ChangeQuotationApproval(false);
                SetActiveButtons();
            }
            else if (msg.Content.Type == NotifType.SystemInfo) // system info changed
            {
                RaisePropertyChangedEvent("NumUsers");
                RaisePropertyChangedEvent("NumLoggedUsers");
                RaisePropertyChangedEvent("NumSysDiginotes");
                RaisePropertyChangedEvent("DiginotesOffer");
                RaisePropertyChangedEvent("DiginotesDemand");
            }
        }

        // function that performs the logout
        private void Logout(object parameter)
        {
            // stop diggings
            StopDigging();

            // logout
            client.Logout();

            // notify 
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LogOut, null), "");
        }

        // when window closes perform logout
        private void Close(object parameter) { client.Logout(); }

        // function that adds a sell order
        private void Sell(object parameter)
        {
            // get quantity
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            if (quantity > 0) // if valid quantity
            {
                // deactivate buttons
                CanSell = CanBuy = CanLower = CanRise = CanRemove = false;

                // reset quantity box
                quantityBox.Value = 0;

                // add sell order
                client.SellDiginotes(quantity);
            }
            else // show error dialog
                System.Windows.MessageBox.Show(Parent, "Provide a quantity greater than zero!", "Zero quantity", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Buy(object parameter)
        {
            // get quantity
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            if (quantity > 0)
            {
                // deactivate buttons
                CanBuy = CanSell = CanLower = CanRise = CanRemove = false;

                // reset quantity box
                quantityBox.Value = 0;

                // add buy order
                client.BuyDiginotes(quantity);
            }
            else // show error dialog
                System.Windows.MessageBox.Show(Parent, "Provide a quantity greater than zero!", "Zero quantity", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // function that warns main window to show change quotation dialog to user
        private void ChangeQuotation(object parameter)
        {
            // get operation
            string operation = parameter as string;

            // show the dialog with the correct parameter
            if (operation == "Lower")
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "-" + Quotation);
            else
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "+" + Quotation);
        }

        // function that removes the current order
        private void RemoveOrder(object parameter) { client.RemoveOrder(); }

        // function that enables or disables interface buttons acordingly to data
        private void SetActiveButtons()
        {
            if(Orders.Count > 0)
            {
                Order lastOrder = Orders[0];
                if (lastOrder.State == OrderState.Over || lastOrder.State == OrderState.Removed)
                {
                    if (DiginotesNumber > 0)
                        CanSell = true;
                    else
                        CanSell = false;
                    CanBuy = true;
                    CanRise = CanLower = CanRemove = false;
                }
                else if (lastOrder.State == OrderState.Pending || lastOrder.State == OrderState.WaitApproval)
                {
                    CanRemove = true;
                    if (lastOrder.Type == OrderType.Buy)
                        CanRise = true;
                    else
                        CanLower = true;
                }
            }
            else
            {
                if (DiginotesNumber > 0)
                    CanSell = true;
                else
                    CanSell = false;
                CanBuy = true;
                CanRise = CanLower = CanRemove = false;
            }
        }

        // function that starts or stops digging
        private void Dig(object parameter)
        {
            if (digging)
                StopDigging();
            else
                StartDigging();

            RaisePropertyChangedEvent("DigButtonText");
        }

        // function that starts digging diginotes
        private void StartDigging()
        {
            if (timer == null) // if never started
            {
                // get the time to dig a new diginote
                client.GetDigSeed();
                
                // create a new timer
                timer = new DispatcherTimer();

                // reset dig time and step
                DigStep = 0;
                digTime = 0;
            }

            // set intervale of update and start timer
            timer.Interval = TimeSpan.FromSeconds(1d);
            timer.Tick += DigTick;
            timer.Start();

            // set digging
            digging = true;
        }

        // function that stops digging diginotes
        private void StopDigging()
        {
            if (timer != null) // if not already stoped
            {
                // stop timer
                timer.Tick -= DigTick;
                timer.Stop();

                // set digging
                digging = false;
            }
        }

        // handler that updates the dig step and gets diginote if time ended
        private void DigTick(object sender, EventArgs e)
        {
            // increment dig time by 1
            digTime++;

            // calculate new dig step
            DigStep = (double)digTime / client.DigTime;

            if (DigStep >= 1.0) // if digstep > 1
            {
                // diggeg diginote
                client.GetDiginoteDigged();

                // reset time and step
                DigStep = 0;
                digTime = 0;
            }
        }

        // function that shows the user the dialog to approve keeping an order after login
        private void QueryApproval(object sender, EventArgs e)
        {
            // stop the timer
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Tick -= QueryApproval;
            timer.Stop();

            // notify main window to show the dialog
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskApprove, null), "");
        }

        public Window Parent { get; set; } // main window instance
    }
}
