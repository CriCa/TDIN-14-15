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

namespace Client.ViewModel
{
    class MainViewModel : BaseViewModel
    {
        private string username = "";
        private double quotation;
        private int diginotesNumber;
        private ObservableCollection<DiginoteInfo> diginotes;
        private ObservableCollection<Order> orders;
        private bool canSell;
        private bool canBuy;
        private bool canLower;
        private bool canRise;

        public ICommand LogoutCommand { get { return new RelayCommand(Logout); } }

        public ICommand DigCommand { get { return new RelayCommand(Dig); } }

        public ICommand SellCommand { get { return new RelayCommand(Sell); } }

        public ICommand BuyCommand { get { return new RelayCommand(Buy); } }

        public ICommand ViewCommand { get { return new RelayCommand(View); } }

        public ICommand ChangeQuotationCommand { get { return new RelayCommand(ChangeQuotation); } }

        public ICommand CloseCommand { get { return new RelayCommand(Close); } }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                RaisePropertyChangedEvent("Username");
            }
        }

        public double Quotation
        {
            get
            {
                return quotation;
            }
            set
            {
                quotation = value;
                RaisePropertyChangedEvent("Quotation");
            }
        }

        public int DiginotesNumber
        {
            get
            {
                return diginotesNumber;
            }
            set
            {
                diginotesNumber = value;
                RaisePropertyChangedEvent("DiginotesNumber");
            }
        }

        public bool CanSell
        {
            get
            {
                return canSell;
            }
            set
            {
                canSell = value;
                RaisePropertyChangedEvent("CanSell");
            }
        }

        public bool CanBuy
        {
            get
            {
                return canBuy;
            }
            set
            {
                canBuy = value;
                RaisePropertyChangedEvent("CanBuy");
            }
        }

        public bool CanLower
        {
            get
            {
                return canLower;
            }
            set
            {
                canLower = value;
                RaisePropertyChangedEvent("CanLower");
            }
        }

        public bool CanRise
        {
            get
            {
                return canRise;
            }
            set
            {
                canRise = value;
                RaisePropertyChangedEvent("CanRise");
            }
        }

        public ObservableCollection<Order> Orders { get { return orders; } }

        public ObservableCollection<DiginoteInfo> Diginotes { get { return diginotes; } }

        public ObservableCollection<DataPoint> QuotationEvolution { get; private set; }

        public ObservableCollection<DataPoint> Transactions { get; private set; }

        public MainViewModel()
        {
            Username = client.user.Name;
            Quotation = client.Quotation;
            diginotes = client.Diginotes;
            DiginotesNumber = client.DiginotesNumber;
            orders = client.Orders;
            CanSell = CanBuy = true;
            CanLower = CanRise = false;
            QuotationEvolution = new ObservableCollection<DataPoint>();
            Transactions = new ObservableCollection<DataPoint>();

            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);
        }

        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.NOSERVER)
            {
                System.Windows.MessageBox.Show("Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(-1);
            }
            else if (msg.Content.Type == NotifType.LOGOUT)
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
            else if (msg.Content.Type == NotifType.QUOTATION)
                Quotation = client.Quotation;
            else if (msg.Content.Type == NotifType.DIGINOTES)
            {
                DiginotesNumber = client.DiginotesNumber;
                diginotes = client.Diginotes;
            }
            else if (msg.Content.Type == NotifType.SETNEWQUOTATION)
                client.SetNewQuotation(Double.Parse((string)msg.Notification));
            else if (msg.Content.Type == NotifType.NEWORDER)
            {
                Order lastOrder = Orders[Orders.Count - 1];
                if(lastOrder.State == OrderState.Over)
                {
                    CanSell = CanBuy = true;
                    CanRise = CanLower = false;
                }
                else if (lastOrder.State == OrderState.Pending)
                {
                    if (lastOrder.Type == OrderType.Buy)
                    {
                        CanRise = true;
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUERYNEWQUOTATION, null), "+" + Quotation);
                    }
                    else
                    {
                        CanLower = true;
                        NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUERYNEWQUOTATION, null), "-" + Quotation);
                    }
                    
                }
            }
        }

        private void Logout(object parameter)
        {
            client.Logout();
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LOGOUT, null), "");
        }

        private void Dig(object parameter)
        {
            client.SetNewQuotation(2.3);
            client.Orders.Add(new Order(OrderType.Buy, 2, client.user));
            client.Diginotes.Add(new DiginoteInfo(6, 2.0, DateTime.Now.ToString()));
            QuotationEvolution.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), 1.1));
            Transactions.Add(new DataPoint(new Random().NextDouble() * 2.0, (int) (new Random().NextDouble() * 10)));
            Console.WriteLine("Dig");
        }

        private void Sell(object parameter)
        {
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            if(quantity > 0) {
                CanSell = CanBuy = CanLower = CanRise = false;
                client.SellDiginotes(quantity);
            }
            else
                System.Windows.MessageBox.Show("Provide a quantity greater than zero!", "Zero quantity", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Buy(object parameter)
        {
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            if (quantity > 0)
            {
                CanBuy = CanSell = CanLower = CanRise = false;
                client.BuyDiginotes(quantity);
            }
            else
                System.Windows.MessageBox.Show("Provide a quantity greater than zero!", "Zero quantity", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void View(object parameter)
        {
            Console.WriteLine("View");
        }

        private void ChangeQuotation(object parameter)
        {
            string operation = parameter as string;
            if (operation == "Lower")
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUERYNEWQUOTATION, null), "-" + Quotation);
            else
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.QUERYNEWQUOTATION, null), "+" + Quotation);
        }

        private void Close(object parameter)
        {
            client.Logout();
        }
    }
}
