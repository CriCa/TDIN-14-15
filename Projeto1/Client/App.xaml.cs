using System;
using Client.Model;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // ClientModel instance
        public ClientModel client = new ClientModel();

        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
