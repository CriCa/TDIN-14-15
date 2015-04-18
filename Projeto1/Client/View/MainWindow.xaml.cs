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
            // subscribe messenger
            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);

            InitializeComponent();

            // set parent in viewmodel
            ((MainViewModel)DataContext).Parent = this;
        }

        // handler for incoming messenger notifications
        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.LogOut) // logout
            {
                // open login window
                new LoginWindow().Show();

                // unregister
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);

                // close current
                this.Close();
            }
            else if (msg.Content.Type == NotifType.AskQuotation) // ask user for new quotation
            {
                double quot = Double.Parse(msg.Notification); // get value
                // create new dialog
                ChangeQuotationDialog dialog = new ChangeQuotationDialog(this, Math.Abs(quot), quot >= 0);

                if (dialog.ShowDialog() == true) // show dialog, if user clicks ok then suggest new quotation
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.SetQuotation, null), dialog.NewQuotation.ToString());
            }
            else if (msg.Content.Type == NotifType.AskApprove) // ask user to approve keeping the offer and send to server the result
            {
                if (new ApproveChangeDialog(this, 60d).ShowDialog() == true)
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ApproveQuotation, null), "Approve");
                else
                    NotificationMessenger.sendNotification(this, new NotificationType(NotifType.ApproveQuotation, null), "Disapprove");
            }
        }
    }
}
