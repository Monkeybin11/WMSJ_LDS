using CPAS.Interface;
using GalaSoft.MvvmLight.Messaging;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CPAS.Views
{
    /// <summary>
    /// UC_CameraView.xaml 的交互逻辑
    /// </summary>
    public partial class UC_CameraView : UserControl , Iauthor
    {
        public UC_CameraView()
        {
            InitializeComponent();
        }
        private bool bFirstLoaded = true;
        public int Level { get; set; }
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(UC_CameraView));
        public int GetLever()
        {
            throw new NotImplementedException();
        }

        public void SetLever(int nLever)
        {
            throw new NotImplementedException();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private async void LoadDelay()
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(1500).Wait();
                    bFirstLoaded = false;
                }
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam1",Cam1.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam2", Cam2.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam3", Cam3.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam4", Cam4.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam5", Cam5.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam6", Cam6.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam7", Cam7.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam8", Cam8.HalconWindow);
                Console.WriteLine("---------------------CameraView--------------------");
            });
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                LoadDelay();
            else
            {
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam1");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam2");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam3");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam4");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam5");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam6");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam7");
                Vision.Vision.Instance.DetachCamWindow(0, "CameraViewCam8");
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Messenger.Default.Send("","WindowChanged");
        }

        private void BenMenuShowImageInCurrentPage_Click(object sender, RoutedEventArgs e)
        {
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam1", Cam1.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam2", Cam2.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam3", Cam3.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam4", Cam4.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam5", Cam5.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam6", Cam6.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam7", Cam7.HalconWindow);
            Vision.Vision.Instance.AttachCamWIndow(0, "CameraViewCam8", Cam8.HalconWindow);
        }
    }
}
