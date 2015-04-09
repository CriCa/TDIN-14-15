using System;
using System.Text;
using System.Security.Cryptography;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Client.ViewModel
{
    class LoginViewModel : BaseViewModel
    {
        private string username;
        private string usernameReg;
        private string nameReg;        

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
                MessageBox.Show("blank");
            else if (true/*client.Login(Username, password)*/)
            {
                // notify to change interface
            }
            else
            {
                MessageBox.Show("wrong");
            }
        }
    }
}
