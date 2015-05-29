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
using StoreApp.BookEditorServices;
using System.Collections.ObjectModel;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for OrdersUserControl.xaml
    /// </summary>
    public partial class OrdersUserControl : UserControlNotifiable
    {
        private App app = Application.Current as App;

        private ObservableCollection<OrderData> orders;

        public ObservableCollection<OrderData> Orders
        {
            get { return orders; }
            set { SetField(ref orders, value, "Orders"); }
        }

        private OrderData selectedOrder;

        public OrderData SelectedOrder
        {
            get { return selectedOrder; }
            set { SetField(ref selectedOrder, value, "SelectedOrder"); }
        }

        public OrdersUserControl()
        {
            InitializeComponent();
            this.DataContext = this;

            init();
        }

        private void init()
        {
            orders = new ObservableCollection<OrderData>();

            app.OrdersEvent += new EventHandler(refreshOrderList);

            refreshOrderList(null, null);
        }

        private void refreshOrderList(object sender, EventArgs e)
        {
            Orders bs = app.clientProxy.getOrders();
            orders.Clear();

            foreach (OrderData b in bs)
                orders.Add(b);
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("do action!");
        }

        private void OrderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("sel changed!");
        }
    }
}
