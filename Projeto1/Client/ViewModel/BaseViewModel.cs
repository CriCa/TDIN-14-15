using System;
using System.ComponentModel;
using Client.Model;

namespace Client.ViewModel
{
    /**
     * Base class for view model that implements the notification of properties changed
     */
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected ClientModel client = (System.Windows.Application.Current as App).client;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
