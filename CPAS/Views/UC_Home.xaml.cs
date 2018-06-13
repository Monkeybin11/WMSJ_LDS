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
        }
        HTuple imageWidth, imageHeight;
        HTuple hv_AcqHandle;
        HObject ho_Image;
        List<HTuple> HwindowList = new List<HTuple>();
        private CancellationTokenSource cts = null;
        private Task task = null;
        private AutoResetEvent grabEvent = new AutoResetEvent(false);
        object _lock = new object();

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
            
            HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
              -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
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
            HwindowList.Add(Cam1.HalconWindow);
            HwindowList.Add(Cam2.HalconWindow);
            HwindowList.Add(Cam3.HalconWindow);
            HwindowList.Add(Cam4.HalconWindow);
            HwindowList.Add(Cam5.HalconWindow);
            HwindowList.Add(Cam6.HalconWindow);
            cts = new CancellationTokenSource();
            task = new Task(() => ThreadFunc(), cts.Token);
            task.Start();
        }

        private void ThreadFunc()
        {

            HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);     //会新生成Image，因为ho_Image初始是为null的，所以GrabImage肯定是内部new了Image
            HOperatorSet.GetImageSize(ho_Image, out HTuple width, out HTuple height);
            ho_Image.Dispose();
            for (int i = 0; i < HwindowList.Count; i++)
            {
                HOperatorSet.SetPart(HwindowList[i], 0, 0, height, width);
                HOperatorSet.SetLineWidth(HwindowList[i], 2);
                HOperatorSet.SetColor(HwindowList[i], "red");
                HOperatorSet.SetDraw(HwindowList[i], "margin");
            }
            HOperatorSet.GenRegionLine(out HObject reg, height / 2, 0, height / 2, width);
            HOperatorSet.GenRegionLine(out HObject reg1, 0, width / 2, height, width / 2);
            HOperatorSet.GenRectangle1(out HObject rect, height / 2 - 50, width / 2 - 50, height / 2 + 50, width / 2 + 50);
            
            while (!cts.Token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    bool ret = grabEvent.WaitOne(10);
                    if (ret)
                        continue;
                    HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);
                    for (int i = 0; i < HwindowList.Count; i++)
                    {
                        HOperatorSet.DispObj(ho_Image, HwindowList[i]);
                        HOperatorSet.DispObj(reg, HwindowList[i]);
                        HOperatorSet.DispObj(reg1, HwindowList[i]);
                        HOperatorSet.DispObj(rect, HwindowList[i]);
                    }
                    ho_Image.Dispose();
                }
            }
            reg.Dispose();
            reg1.Dispose();
            rect.Dispose();
        }
    }
}
