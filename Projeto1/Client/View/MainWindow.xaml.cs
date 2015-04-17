using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using Client.Helpers;
using Client.ViewModel;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);

            InitializeComponent();

            ((MainViewModel)DataContext).Parent = this;
        }

        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.LogOut)
            {
                // open login window
                new LoginWindow().Show();

                // unregister
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);

                // close current
                this.Close();
            }
            else if (msg.Content.Type == NotifType.AskQuotation)
            {
                double quot = Double.Parse(msg.Notification);
                ChangeQuotationDialog dialog = new ChangeQuotationDialog(this, Math.Abs(quot), quot >= 0);

                if (dialog.ShowDialog() == true)
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SetQuotation, null), dialog.NewQuotation.ToString());
            }
            else if (msg.Content.Type == NotifType.AskApprove)
            {
                if (new ApproveChangeDialog(this, 10d).ShowDialog() == true)
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ApproveQuotation, null), "Approve");
                else
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ApproveQuotation, null), "Disapprove");
            }
        }
    }
}
