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

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for BookDialog.xaml
    /// </summary>
    public partial class BookDialog : Window
    {
        public string BookTitle { get; set; }

        public long BookQuantity { get; set; }

        public double BookPrice { get; set; }


        public BookDialog(Window parent)
        {
            this.Owner = parent;

            InitializeComponent();

            Title = "Add book";

            QuantityBox.Minimum = 1;
            QuantityBox.Value = 1;
        }

        public BookDialog(Window parent, string title, long quantity, double price)
        {
            this.Owner = parent;

            InitializeComponent();

            Title = "Update book info";

            TitleBox.Text = title;
            QuantityBox.Value = (int) quantity;
            PriceBox.Value = price;

            SubmitButton.Content = "Update";

        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            BookTitle = TitleBox.Text;
            BookQuantity = (long) QuantityBox.Value;
            BookPrice = (double) PriceBox.Value;

            DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
