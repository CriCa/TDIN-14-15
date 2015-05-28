using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for CreateOrderDialog.xaml
    /// </summary>
    public partial class CreateOrderDialog : Window
    {
        public string ClientEmail { get; set; }

        public CreateOrderDialog(Window par)
        {
            this.Owner = par;

            InitializeComponent();
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            ClientEmail = email.Text;

            DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckEmail(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(email.Text,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase))
                SubmitButton.IsEnabled = true;
            else SubmitButton.IsEnabled = false;
        }
    }
}
