using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using WarehouseApp.ServiceWarehouse;

namespace WarehouseApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ChannelFactory<IServiceWarehouse> chanFactory;
        public IServiceWarehouse clientProxy;

        public App() { }
    }
}
