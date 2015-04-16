using System;
using System.Text;
using System.Security.Cryptography;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Client.Helpers;

namespace Client.ViewModel
{
    class LoginViewModel : BaseViewModel
    {
        private string username = "";
        private string usernameReg = "";
        private string nameReg = "";

        private bool noserver;

        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                RaisePropertyChangedEvent("Username");
            }
        }

        public string UsernameRegister
        {
            get { return usernameReg; }
            set
            {
                usernameReg = value;
                RaisePropertyChangedEvent("UsernameRegister");
            }
        }

        public string NameRegister
        {
            get { return nameReg; }
            set
            {
                nameReg = value;
                RaisePropertyChangedEvent("NameRegister");
            }
        }

        public LoginViewModel()
        {
            noserver = false;
            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);
        }

        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.NOSERVER)
                noserver = true;
            else if(msg.Content.Type == NotifType.LOGIN)
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
        }

        private string Encrypt(string password)
        {
            StringBuilder builder = new StringBuilder();
            SHA256 sha256 = SHA256.Create();

            byte[] passwordHash = sha256.ComputeHash(Encoding.Default.GetBytes(password));

            for (int i = 0; i < passwordHash.Length; ++i)
                builder.Append(passwordHash[i].ToString());

            return builder.ToString();
        }

        public ICommand LoginCommand { get { return new RelayCommand(Login); } }

        private void Login(object parameter)
        {
            PasswordBox pBox = parameter as PasswordBox;
            string password = pBox.Password;

            if (Username.Length == 0 || password.Length == 0)
                MessageBox.Show(Application.Current.MainWindow, "Provide username and password in order to login!", "Missing fields", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (client.Login(Username, Encrypt(password)))
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LOGIN, null), "DEFAULT");
            else
            {
                if (!noserver)
                    MessageBox.Show(Application.Current.MainWindow, "Wrong user/password information or already logged in.\nPlease try again!", "Wrong login", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    MessageBox.Show(Application.Current.MainWindow, "Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Environment.Exit(-1);
                }
            }
        }

        public ICommand RegisterCommand { get { return new RelayCommand(Register); } }

        private void Register(object parameter)
        {
            Tuple<PasswordBox, PasswordBox> param = parameter as Tuple<PasswordBox, PasswordBox>;
            string password = ((PasswordBox)param.Item1).Password;
            string passwordConfirm = ((PasswordBox)param.Item2).Password;

            if (UsernameRegister == "" || NameRegister == "" || password == "" || passwordConfirm == "")
                MessageBox.Show(Application.Current.MainWindow, "Please provide all fields in order to register!", "Missing fields", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (UsernameRegister.Length < 3)
                MessageBox.Show(Application.Current.MainWindow, "Your username must at least 3 characters!", "Bad username", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (password != passwordConfirm)
                MessageBox.Show(Application.Current.MainWindow, "Passwords don't match! Please make to confirm your password!", "Wrong passwords", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (client.Register(NameRegister, UsernameRegister, Encrypt(password)))
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.LOGIN, null), "DEFAULT");
            else
            {
                if (!noserver)
                    MessageBox.Show(Application.Current.MainWindow, "Username already taken! Please choose another username!", "Username taken", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    MessageBox.Show(Application.Current.MainWindow, "Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Environment.Exit(-1);
                }
            }
        }
    }
}
