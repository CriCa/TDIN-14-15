using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using StoreApp.BookEditorServices;
using System.Windows;

namespace StoreApp
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class ServiceStoreCallback : IServiceStoreCallback
    {
        App app = Application.Current as App;
        public void test(int t)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Console.WriteLine("Callback!!!");
            }));

        }
    }
}
