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
using System.ServiceModel;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        App app = Application.Current as App;

        public MainWindow()
        {
            init();
            try { 
            InitializeComponent();
            }
            catch (Exception e) { Console.WriteLine(e.InnerException); }
        }

        private void init()
        {
            app.callback = new ServiceStoreCallback();
            app.dupFactory =
                new DuplexChannelFactory<BookEditorServices.IServiceStore>(
                app.callback, new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:9002/BookEditorServices/store"));

            app.dupFactory.Open();
            app.clientProxy = app.dupFactory.CreateChannel();
        }
    }
}
