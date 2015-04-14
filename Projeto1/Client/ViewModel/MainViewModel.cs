using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Helpers;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using System.Windows.Controls;

namespace Client.ViewModel
{
    class MainViewModel : BaseViewModel
    {
        private string username = "";

        public ICommand LogoutCommand { get { return new RelayCommand(Logout); } }

        public string Username 
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                RaisePropertyChangedEvent("PropertyChanged");
            }
        }

        public MainViewModel()
        {
            Username = client.user.Username;

            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);
        }

        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if(msg.Content.Type == NotifType.LOGOUT)
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
        }

        private void Logout(object parameter)
        {
            NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LOGOUT, null), "");
        }
    }
}
