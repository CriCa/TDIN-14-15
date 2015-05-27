using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.ServiceModel;
using System.Threading.Tasks;
using StoreApp.BookEditorServices;

namespace StoreApp
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ServiceStoreCallback callback;
        public DuplexChannelFactory<IServiceStore> dupFactory;
        public IServiceStore clientProxy;

        public App() { }
    }
}
