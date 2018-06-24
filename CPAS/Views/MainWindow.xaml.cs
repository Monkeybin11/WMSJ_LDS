using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using CPAS.Interface;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using CPAS.Config;
using GalaSoft.MvvmLight.Messaging;

namespace CPAS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task taskMonitor = null;
        private CancellationTokenSource cts = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (taskMonitor == null || taskMonitor.IsCompleted || taskMonitor.IsCanceled)
            {
                DataTable dtError = ConfigMgr.PLCErrorDataTable;
                cts = new CancellationTokenSource();
                taskMonitor = new Task(() => {
                    while (!cts.IsCancellationRequested)
                    {
                        foreach (DataRow it in dtError.Rows)
                        {
                            Messenger.Default.Send<string>(it["RegisterBit"].ToString() +"  "+ it["ErrorMessage"].ToString(),"ShowError");
                            Thread.Sleep(300);
                        }
                    }
                },cts.Token);
            }
            //taskMonitor.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BtnStop.PerformClick();
            //if (cts != null)
            //{
            //    cts.Cancel();
            //    taskMonitor.Wait(2000);
            //}
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
