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

namespace WarehouseApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        App app = Application.Current as App;

        private ObservableCollection<Request> requests;

        public ObservableCollection<Request> Requests
        {
            get { return requests; }
            set { SetField(ref requests, value, "Requests"); }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            init();
        }

        private void init()
        {
            requests = new ObservableCollection<Request>();

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
            
            ServiceRequest.RequestEvent += new EventHandler(refreshQuestions);
            
            host.Open();

            app.chanFactory =
                new ChannelFactory<IServiceWarehouse>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:9002/BookEditorServices/warehouse"));

            app.chanFactory.Open();
            app.clientProxy = app.chanFactory.CreateChannel();

            /*Request req = new Request(1, "a", 2, 0, "adsad", "asdasd");
            req.id = 1;
            req.title = "cenas";
            req.quantity = 2;
            req.state = 0;
            req.date = "cead";
            req.state_date = "dasda";

            Console.WriteLine(req.GetHashCode());
            requests.Add(req);
            requests.Add(new Request(1, "a", 2, 0, "adsad", "asdasd"));
            requests.Add(new Request(1, "Asdss", 2, 0, "adsad", "asdasd"));

            Console.WriteLine(req.title);*/
        }

        private void refreshQuestions(object sender, EventArgs e)
        {
            Console.WriteLine("refresh questions");
        }

        private void ShipRequest(object sender, RoutedEventArgs e)
        {
            BookData b = new BookData();

            app.clientProxy.ship(b, 2);
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
