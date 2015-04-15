using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Helpers;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using System.Collections.ObjectModel;

namespace Client.ViewModel
{
    class MainViewModel : BaseViewModel
    {
        private string username = "";
        private double quotation;
        private int diginotesNumber;
        private ObservableCollection<string> orders;

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

        public ObservableCollection<string> Orders { get { return orders; } }

        public MainViewModel()
        {
            Username = client.user.Name;
            Quotation = client.Quotation;
            DiginotesNumber = client.DiginotesNumber;
            this.orders = client.orders;

            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);
        }

        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.LOGOUT)
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
            else if (msg.Content.Type == NotifType.QUOTATION)
                Quotation = client.Quotation;
            else if (msg.Content.Type == NotifType.DIGINOTESNUMBER)
                DiginotesNumber = client.DiginotesNumber;
        }

        private void Logout(object parameter)
        {
            client.Logout();
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LOGOUT, null), "");
        }

        private void Dig(object parameter)
        {
            client.SetNewQuotation(2.3);
            client.orders.Add("test");
            Console.WriteLine("Dig");
        }

        private void Sell(object parameter)
        {
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            Console.WriteLine("Sell " + quantity);
        }

        private void Buy(object parameter)
        {
            IntegerUpDown quantityBox = parameter as IntegerUpDown;
            int quantity = (int)quantityBox.Value;

            Console.WriteLine("Buy " + quantity);
        }

        private void View(object parameter)
        {
            Console.WriteLine("View");
        }

        private void ChangeQuotation(object parameter)
        {
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SUGGESTQUOTATION, null), "");
        }

        private void Close(object parameter)
        {
            client.Logout();
        }
    }
}
