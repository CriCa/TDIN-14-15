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

namespace Client.View
{
    /// <summary>
    /// Interaction logic for ChangeQuotationDialog.xaml
    /// </summary>
    public partial class ChangeQuotationDialog : Window
    {
        public ChangeQuotationDialog(Window own, double quotation, bool rise)
        {
            InitializeComponent();

            // set parent
            Owner = own;

            if (rise)
            {
                NewQuotation = MinQuotation = quotation + 0.01;
                MaxQuotation = 9999;
            }
            else
            {
                MinQuotation = 0.01;
                NewQuotation = MaxQuotation = quotation - 0.01;
            }

        }

        public double NewQuotation
        {
            get { return (double)NewQuotationBox.Value; }
            set { NewQuotationBox.Value = value; }
        }

        public double MinQuotation
        {
            get { return (double)NewQuotationBox.Minimum; }
            set { NewQuotationBox.Minimum = value; }
        }

        public double MaxQuotation
        {
            get { return (double)NewQuotationBox.Maximum; }
            set { NewQuotationBox.Maximum = value; }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
