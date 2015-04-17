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
        private ObservableCollection<DiginoteInfo> diginotes;

        private ObservableCollection<Order> orders;

        private bool canSell;
        private bool canBuy;

        private bool canLower;
        private bool canRise;

        private bool canRemove;

        private bool digging;
        private double digStep;
        private int digTime;
        private DispatcherTimer timer;

        public ICommand LogoutCommand { get { return new RelayCommand(Logout); } }

        public ICommand DigCommand { get { return new RelayCommand(Dig); } }

        public ICommand SellCommand { get { return new RelayCommand(Sell); } }

        public ICommand BuyCommand { get { return new RelayCommand(Buy); } }

        public ICommand ViewCommand { get { return new RelayCommand(View); } }

        public ICommand ChangeQuotationCommand { get { return new RelayCommand(ChangeQuotation); } }

        public ICommand CloseCommand { get { return new RelayCommand(Close); } }

        public ICommand RemoveCommand { get { return new RelayCommand(RemoveOrder); } }

        public string Username { get { return client.user.Name; } }

        public double Quotation { get { return client.Quotation; } }

        public double DiginotesNumber { get { return client.Diginotes.Count; } }

        public string DigButtonText { get { if (digging) return "Stop"; else return "Start"; } }

        public int NumUsers { get { return client.NumUsers; } }

        public int NumLoggedUsers { get { return client.NumLoggedUsers; } }

        public int NumSysDiginotes { get { return client.NumSysDiginotes; } }

        public int DiginotesOffer { get { return client.DiginotesOffer; } }

        public int DiginotesDemand { get { return client.DiginotesDemand; } }

        public ObservableCollection<Order> Orders { get { return orders; } }

        public ObservableCollection<DiginoteInfo> Diginotes { get { return diginotes; } }

        public ObservableCollection<DataPoint> QuotationEvolution { get { return client.QuotationEvolution; } }

        public ObservableCollection<DataPoint> TransactionsPerMin { get { return client.TransactionsPerMin; } }

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

        public MainViewModel()
        {
            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);

            diginotes = client.Diginotes;
            orders = client.Orders;

            SetActiveButtons();

            digging = false;

            if (Orders.Count > 0 && Orders[0].State == OrderState.WaitApproval) { 
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1d);
                    timer.Tick += QueryApproval;
                    timer.Start();
            }
        }

        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.NoServer)
            {
                System.Windows.MessageBox.Show(Parent, "Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(-1);
            }
            else if (msg.Content.Type == NotifType.LogOut)
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
            else if (msg.Content.Type == NotifType.Quotation)
                RaisePropertyChangedEvent("Quotation");
            else if (msg.Content.Type == NotifType.Diginotes)
                RaisePropertyChangedEvent("DiginotesNumber");
            else if (msg.Content.Type == NotifType.SetQuotation)
                client.SetNewQuotation(Double.Parse((string)msg.Notification));
            else if (msg.Content.Type == NotifType.Order || msg.Content.Type == NotifType.Transaction)
                SetActiveButtons();
            else if (msg.Content.Type == NotifType.ApproveQuotation)
            {
                if (msg.Notification == "Approve")
                    client.ChangeQuotationApproval(true);
                else
                    client.ChangeQuotationApproval(false);
                SetActiveButtons();
            }
            else if (msg.Content.Type == NotifType.SystemInfo)
            {
                RaisePropertyChangedEvent("NumUsers");
                RaisePropertyChangedEvent("NumLoggedUsers");
                RaisePropertyChangedEvent("NumSysDiginotes");
                RaisePropertyChangedEvent("DiginotesOffer");
                RaisePropertyChangedEvent("DiginotesDemand");
            }
        }

        private void Logout(object parameter)
        {
            StopDigging();
            client.Logout();
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LogOut, null), "");
        }

        private void Dig(object parameter)
        {
            if (digging)
                StopDigging();
            else
                StartDigging();
            
            RaisePropertyChangedEvent("DigButtonText");
        }

        private void StartDigging()
        {
            if (timer == null)
            {
                client.GetDigSeed();
                timer = new DispatcherTimer();
                DigStep = 0;
                digTime = 0;
            }

            timer.Interval = TimeSpan.FromSeconds(1d);
            timer.Tick += DigTick;
            timer.Start();

            digging = true;
        }

        private void StopDigging()
        {
            if (timer != null)
            { 
                timer.Tick -= DigTick;
                timer.Stop();

                digging = false;
            }
        }

        private void DigTick(object sender, EventArgs e)
        {
            digTime++;

            DigStep = (double)digTime / client.DigTime;

            if (DigStep >= 1.0) {
                client.GetDiginoteDigged();
                DigStep = 0;
                digTime = 0;
            }
        }

        private void Sell(object parameter)
        {
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            if (quantity > 0)
            {
                CanSell = CanBuy = CanLower = CanRise = CanRemove = false;
                client.SellDiginotes(quantity);
                quantityBox.Value = 0;
            }
            else
                System.Windows.MessageBox.Show(Parent, "Provide a quantity greater than zero!", "Zero quantity", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Buy(object parameter)
        {
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            if (quantity > 0)
            {
                CanBuy = CanSell = CanLower = CanRise = CanRemove = false;
                client.BuyDiginotes(quantity);
                quantityBox.Value = 0;
            }
            else
                System.Windows.MessageBox.Show(Parent, "Provide a quantity greater than zero!", "Zero quantity", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void View(object parameter)
        {
            Console.WriteLine("View");
        }

        private void ChangeQuotation(object parameter)
        {
            string operation = parameter as string;
            if (operation == "Lower")
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "-" + Quotation);
            else
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskQuotation, null), "+" + Quotation);
        }

        private void Close(object parameter)
        {
            client.Logout();
        }

        private void RemoveOrder(object parameter)
        {
            client.RemoveOrder();
        }

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

        private void QueryApproval(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= QueryApproval;

            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.AskApprove, null), "");
        }

        public Window Parent { get; set; }
    }
}
