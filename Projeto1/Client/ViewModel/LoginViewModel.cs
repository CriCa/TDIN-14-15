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
        private string username = ""; // username in login control
        private string usernameReg = ""; // username in register control
        private string nameReg = ""; // name in register control

        private bool noserver; // no server flag

        public string Username
        {
            get { return username; }
            set { username = value; RaisePropertyChangedEvent("Username"); }
        }

        public string UsernameRegister
        {
            get { return usernameReg; }
            set { usernameReg = value; RaisePropertyChangedEvent("UsernameRegister"); }
        }

        public string NameRegister
        {
            get { return nameReg; }
            set { nameReg = value; RaisePropertyChangedEvent("NameRegister"); }
        }

        // constructor
        public LoginViewModel()
        {
            // initialize
            noserver = false;
            
            // subscribe messenger
            Messenger.Default.Register<NotificationMessage<NotificationType>>(this, NotificationMessageHandler);
        }

        // handler for incoming messenger notifications
        private void NotificationMessageHandler(NotificationMessage<NotificationType> msg)
        {
            if (msg.Content.Type == NotifType.NoServer) // can't reach server
                noserver = true;
            else if(msg.Content.Type == NotifType.Login) // login was successful
                Messenger.Default.Unregister<NotificationMessage<NotificationType>>(this);
        }

        // function that encrypts a password using SHA256 cypher
        private string Encrypt(string password)
        {
            StringBuilder builder = new StringBuilder();
            SHA256 sha256 = SHA256.Create();

            byte[] passwordHash = sha256.ComputeHash(Encoding.Default.GetBytes(password));

            for (int i = 0; i < passwordHash.Length; ++i)
                builder.Append(passwordHash[i].ToString());

            return builder.ToString();
        }

        public ICommand LoginCommand { get { return new RelayCommand(Login); } } // login command

        // function that try login and produces the changes acordingly (change to main window or warn the user about the error)
        private void Login(object parameter)
        {
            // get password
            PasswordBox pBox = parameter as PasswordBox;
            string password = pBox.Password;

            if (Username.Length == 0 || password.Length == 0) // if one of the fields isn't fulfilled
                MessageBox.Show(Application.Current.MainWindow, "Provide username and password in order to login!", "Missing fields", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (client.Login(Username, Encrypt(password))) // if succesfully logged
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Login, null), "DEFAULT");
            else // else
            {
                if (!noserver) // wrong login info or already logged
                    MessageBox.Show(Application.Current.MainWindow, "Wrong user/password information or already logged in.\nPlease try again!", "Wrong login", MessageBoxButton.OK, MessageBoxImage.Error);
                else // can't reach server
                {
                    MessageBox.Show(Application.Current.MainWindow, "Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Environment.Exit(-1);
                }
            }
        }

        public ICommand RegisterCommand { get { return new RelayCommand(Register); } } // register command

        // function that try resgiter and produces the changes acordingly (change to main window or warn the user about the error)
        private void Register(object parameter)
        {
            // get passwords
            Tuple<PasswordBox, PasswordBox> param = parameter as Tuple<PasswordBox, PasswordBox>;
            string password = ((PasswordBox)param.Item1).Password;
            string passwordConfirm = ((PasswordBox)param.Item2).Password;

            if (UsernameRegister == "" || NameRegister == "" || password == "" || passwordConfirm == "") // if one of the fields isn't fulfilled
                MessageBox.Show(Application.Current.MainWindow, "Please provide all fields in order to register!", "Missing fields", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (UsernameRegister.Length < 3) // if username is too short
                MessageBox.Show(Application.Current.MainWindow, "Your username must at least 3 characters!", "Bad username", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (password != passwordConfirm) // if passwords don't match
                MessageBox.Show(Application.Current.MainWindow, "Passwords don't match! Please make to confirm your password!", "Wrong passwords", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (client.Register(NameRegister, UsernameRegister, Encrypt(password))) // if successfully logged
                NotificationMessenger.sendNotification(this, new NotificationType(NotifType.Login, null), "DEFAULT");
            else // else
            {
                if (!noserver) // username already taken
                    MessageBox.Show(Application.Current.MainWindow, "Username already taken! Please choose another username!", "Username taken", MessageBoxButton.OK, MessageBoxImage.Error);
                else // can't reach server
                {
                    MessageBox.Show(Application.Current.MainWindow, "Can't reach server! Exiting Application!", "No server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Environment.Exit(-1);
                }
            }
        }
    }
}
