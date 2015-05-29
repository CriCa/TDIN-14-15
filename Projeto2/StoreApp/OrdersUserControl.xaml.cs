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

        private ObservableCollection<RequestData> requests;

        public ObservableCollection<RequestData> Requests
        {
            get { return requests; }
            set { SetField(ref requests, value, "Requests"); }
        }

        private RequestData selectedRequest;

        public RequestData SelectedRequest
        {
            get { return selectedRequest; }
            set { SetField(ref selectedRequest, value, "SelectedRequest"); }
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
            requests = new ObservableCollection<RequestData>();

            app.OrdersEvent += new EventHandler(refreshOrderList);

            refreshOrderList(null, null);
        }

        private void refreshOrderList(object sender, EventArgs e)
        {
            Orders bs = app.clientProxy.getOrders();
            bs.Reverse();

            orders.Clear();

            foreach (OrderData b in bs)
                orders.Add(b);

            Requests rs = app.clientProxy.getRequests();
            rs.Reverse();

            requests.Clear();

            foreach (RequestData b in rs)
                requests.Add(b);
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedRequest != null)
            {
                app.clientProxy.ReceivedRequest(SelectedRequest);

                refreshOrderList(null, null);
                app.RefreshBooks();
            }
        }

        private void OrderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedRequest != null && SelectedRequest.state == 1)
                ReceiveButton.IsEnabled = true;
            else ReceiveButton.IsEnabled = false;
        }
    }
}
