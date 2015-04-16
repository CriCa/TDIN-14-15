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
        DispatcherTimer timer;

        public ApproveChangeDialog(Window own, double timeout)
        {
            InitializeComponent();

            Owner = own;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(timeout);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Tick -= TimerTick;
            timer = null;

            DialogResult = true;
        }

        private void Disapprove_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Tick -= TimerTick;
            timer = null;

            DialogResult = false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Tick -= TimerTick;
            timer = null;

            DialogResult = true;
        }
    }
}
