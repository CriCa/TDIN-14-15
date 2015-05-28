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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Printer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Window own, string bookTitle, long qtd, double pric, double ttl, string cli)
        {
            InitializeComponent();

            this.Owner = own;

            BookTitle.Content = bookTitle;
            Quantity.Content = qtd;
            Price.Content = "$" + pric.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            Total.Content = "$" + ttl.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            Client.Content = cli;
        }
    }
}
