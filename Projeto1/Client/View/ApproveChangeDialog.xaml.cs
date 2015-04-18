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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for ApproveChangeDialog.xaml
    /// </summary>
    public partial class ApproveChangeDialog : Window
    {
        DispatcherTimer timer; // timer to close window after timeout

        // constructor
        public ApproveChangeDialog(Window own, double timeout)
        {
            InitializeComponent();

            // set owner
            Owner = own;

            // initialize timer and start
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(timeout);
            timer.Tick += TimerTick;
            timer.Start();
        }

        // stop timer and set dialog result to true
        private void Approve_Click(object sender, RoutedEventArgs e) { StopAndResult(true); }

        // stop timer and set dialog result to false
        private void Disapprove_Click(object sender, RoutedEventArgs e) { StopAndResult(false); }

        // stop timer and set dialog result to true
        private void TimerTick(object sender, EventArgs e) { StopAndResult(true); }

        // stop timer and set dialog result to false
        private void WindowClosed(object sender, System.ComponentModel.CancelEventArgs e) { StopAndResult(false); }

        // function that stops the timer and set the dialog result
        private void StopAndResult(bool result)
        {
            if (timer != null) { 
                timer.Stop();
                timer.Tick -= TimerTick;
                timer = null;

                DialogResult = result;
            }
        }

    }
}
