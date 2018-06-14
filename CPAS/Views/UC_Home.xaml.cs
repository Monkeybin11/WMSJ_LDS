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
            Messenger.Default.Register<string>(this, "WindowChanged", str => { lock (_lock) { grabEvent.Set(); } });
        }

        ~UC_Home()
        {
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
            
        }

        private async void LoadDelay()
        {
            await Task.Run(()=> {
                if (bFirstLoaded)
                {
                    Task.Delay(1500).Wait();
                    bFirstLoaded = false;
                }
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam1", Cam1.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam2", Cam2.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam3", Cam3.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam4", Cam4.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam5", Cam5.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam6", Cam6.HalconWindow);
                Console.WriteLine("---------------------Home--------------------");

            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (_lock)
            {
                grabEvent.Set();
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (task == null || task.IsCompleted || task.IsCanceled)
            {
                Vision.Vision.Instance.OpenCam(0);
                cts = new CancellationTokenSource();
                task = new Task(() => ThreadFunc(), cts.Token);
                task.Start();
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                LoadDelay();
            else
            {
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam1");
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam2");
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam3");
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam4");
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam5");
                Vision.Vision.Instance.DetachCamWindow(0, "HomeCam6");
            }
        }

        private void BenMenuShowImageInCurrentPage_Click(object sender, RoutedEventArgs e)
        {
            Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam1", Cam1.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam2", Cam2.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam3", Cam3.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam4", Cam4.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam5", Cam5.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "HomeCam6", Cam6.HalconWindow);
        }

        private void ThreadFunc()
        {

            //HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);     //会新生成Image，因为ho_Image初始是为null的，所以GrabImage肯定是内部new了Image
            //HOperatorSet.GetImageSize(ho_Image, out HTuple width, out HTuple height);
            //ho_Image.Dispose();
            //for (int i = 0; i < HwindowList.Count; i++)
            //{
            //    HOperatorSet.SetPart(HwindowList[i], 0, 0, height, width);
            //    HOperatorSet.SetLineWidth(HwindowList[i], 2);
            //    HOperatorSet.SetColor(HwindowList[i], "red");
            //    HOperatorSet.SetDraw(HwindowList[i], "margin");
            //}
            //HOperatorSet.GenRegionLine(out HObject reg, height / 2, 0, height / 2, width);
            //HOperatorSet.GenRegionLine(out HObject reg1, 0, width / 2, height, width / 2);
            //HOperatorSet.GenRectangle1(out HObject rect, height / 2 - 50, width / 2 - 50, height / 2 + 50, width / 2 + 50);
            
            while (!cts.Token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    bool ret = grabEvent.WaitOne(50);
                    if (ret)
                        continue;
                    //HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);
                    //for (int i = 0; i < HwindowList.Count; i++)
                    //{
                    //    HOperatorSet.DispObj(ho_Image, HwindowList[i]);
                    //    HOperatorSet.DispObj(reg, HwindowList[i]);
                    //    HOperatorSet.DispObj(reg1, HwindowList[i]);
                    //    HOperatorSet.DispObj(rect, HwindowList[i]);
                    //}
                    //ho_Image.Dispose();
                    Vision.Vision.Instance.GrabImage(0);
                    
                }
            }
            Vision.Vision.Instance.CloseCam(0);
            //reg.Dispose();
            //reg1.Dispose();
            //rect.Dispose();
        }
    }
}
