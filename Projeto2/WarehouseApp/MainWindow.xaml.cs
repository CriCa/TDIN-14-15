using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Messaging;
using DatabaseController;
using WarehouseService;
using WarehouseApp.ServiceWarehouse;
using Utilities;

namespace WarehouseApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        App app = Application.Current as App;

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

        public MainWindow()
        {
            this.DataContext = this;
            init();

            InitializeComponent();
        }

        private void init()
        {
            string address = "net.msmq://localhost/private/OrderQueue";
            string queueName = ".\\private$\\OrderQueue";

            LocalDatabase.Instance.open();

            if (!MessageQueue.Exists(queueName))
                MessageQueue.Create(queueName, true);

            NetMsmqBinding binding = new NetMsmqBinding();
            binding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
            binding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;
            
            EndpointAddress endpoint = new EndpointAddress(address);

            ServiceHost host = new ServiceHost(typeof(ServiceRequest), new Uri("http://localhost:9001/WarehouseRequestService/"));
            host.AddServiceEndpoint(typeof(IServiceRequest), binding, address);
            
            host.Open();

            app.chanFactory =
                new ChannelFactory<IServiceWarehouse>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:9002/BookEditorServices/warehouse"));

            app.chanFactory.Open();
            app.clientProxy = app.chanFactory.CreateChannel();

            requests = new ObservableCollection<RequestData>();
            ServiceRequest.RequestEvent += new EventHandler(refreshRequests);

            refreshRequests(null, null);
        }

        private void refreshRequests(object sender, EventArgs e)
        {
            List<Values> reqs = RequestTable.Instance.all;

            reqs.Reverse();

            requests.Clear();

            foreach (Values v in reqs)
            {
                RequestData request = new RequestData();
                request.id = (long)v.getValue(RequestTable.KEY_ID);
                request.order_id = (long)v.getValue(RequestTable.KEY_ORDER_ID);
                request.book_id = (long)v.getValue(RequestTable.KEY_BOOK_ID);
                request.title = (string)v.getValue(RequestTable.KEY_TITLE);
                request.quantity = (long)v.getValue(RequestTable.KEY_QUANTITY);
                request.state = (int)((long)v.getValue(RequestTable.KEY_STATE));
                request.date = (string)v.getValue(RequestTable.KEY_DATE);
                request.state_date = (string)v.getValue(RequestTable.KEY_STATE_DATE);

                requests.Add(request);
            }
        }

        private void ShipRequest(object sender, RoutedEventArgs e)
        {
            if (SelectedRequest != null) { 
                app.clientProxy.ship(SelectedRequest);

                Values values = new Values();
                Values where_values = new Values();

                values.add(RequestTable.KEY_STATE_DATE, Functions.getCurrentDate());
                values.add(RequestTable.KEY_STATE, RequestTable.SHIPPED);

                where_values.add(RequestTable.KEY_ID, SelectedRequest.id);

                RequestTable.Instance.update(values, where_values);

                refreshRequests(null, null);
            }
        }

        private void RequestSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedRequest != null)
            {
                if (SelectedRequest.state == RequestTable.WAITING)
                    ActionButton.IsEnabled = true;
                else
                    ActionButton.IsEnabled = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
