using CPAS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HalconDotNet;
using System.Threading;
using System.Threading.Tasks;
using CPAS.Vision;
using GalaSoft.MvvmLight.Messaging;
using CPAS.ViewModels;


namespace CPAS.Views
{
    /// <summary>
    /// UC_Home.xaml 的交互逻辑
    /// </summary>
    public partial class UC_Home : UserControl , Iauthor
    {
        public UC_Home()
        {
            InitializeComponent();
            Messenger.Default.Register<string>(this, "WindowSizeChanged", str => { lock (_lock) { grabEvent.Set(); } });
            Messenger.Default.Register<Tuple<string,int>>(this, "SetCamState", cmd => {
                lock (_lock) {
                    switch (cmd.Item1.ToLower())
                    {
                        case "snapcontinuous":
                            StartContinusGrab(cmd.Item2);
                            break;
                        case "stopsnap":
                            if (cts != null)
                                cts.Cancel();
                            break;
                        case "snaponce":
                            Vision.Vision.Instance.GrabImage(cmd.Item2);
                            break;
                        default:
                            throw new Exception("Unknow cmd for camera!");
                    }
                }
            });
    
        }

        ~UC_Home()
        {
            Messenger.Default.Unregister("WindowSizeChanged");
            Messenger.Default.Unregister("SetCamState");
            if (cts != null)
            {
                cts.Cancel();
                task.Wait(3000);
            }
            //Vision.Vision.Instance.CloseCam(0);
        }

        List<HTuple> HwindowList = new List<HTuple>();
        private CancellationTokenSource cts = null;
        private Task task = null;
        private AutoResetEvent grabEvent = new AutoResetEvent(false);
        private object _lock = new object();
        private bool bFirstLoaded = true;

        public int Level {get; set; }
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level",typeof(int),typeof(UC_Home));
        public int GetLever()
        {
            return Level;
        }
        public void SetLever(int nLever)
        {
            Level= nLever;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDelay();
        }

        private async void LoadDelay()
        {
            await Task.Run(()=> {
                if (bFirstLoaded)
                {
                    Task.Delay(1500).Wait();
                    bFirstLoaded = false;
                }
                SetAttachWindow(true);
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (_lock)
            {
                grabEvent.Set();
            }
        }

        private void  StartContinusGrab(int nCamID)
        {
            if (task == null || task.IsCompleted || task.IsCanceled)
            {
                cts = new CancellationTokenSource();
                task = new Task(() => ThreadFunc(nCamID), cts.Token);
                task.Start();
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !bFirstLoaded)
                SetAttachWindow(true);
            else
            {
                SetAttachWindow(false);
            }
        }
        private void SetAttachWindow(bool bAttach)
        {
            if (bAttach)
            {
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam1", Cam1.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(1, "HomeCam2", Cam2.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(2, "HomeCam3", Cam3.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(3, "HomeCam4", Cam4.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(4, "HomeCam5", Cam5.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(5, "HomeCam6", Cam6.HalconWindow);
            }
            else
            {
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam1");
                Vision.Vision.Instance.DetachCamWindow(1, "HomeCam2");
                Vision.Vision.Instance.DetachCamWindow(2, "HomeCam3");
                Vision.Vision.Instance.DetachCamWindow(3, "HomeCam4");
                Vision.Vision.Instance.DetachCamWindow(4, "HomeCam5");
                Vision.Vision.Instance.DetachCamWindow(5, "HomeCam6");
            }
        }
        private void ThreadFunc(int nCamID)
        { 
            while (!cts.Token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    bool ret = grabEvent.WaitOne(50);
                    if (ret)
                        continue;

                    Vision.Vision.Instance.GrabImage(nCamID);
                }
            }
            //Vision.Vision.Instance.CloseCam(0);
        }
    }
}
