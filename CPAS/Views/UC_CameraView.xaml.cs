using CPAS.Interface;
using CPAS.ViewModels;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CPAS.Views
{
    /// <summary>
    /// UC_CameraView.xaml 的交互逻辑
    /// </summary>
    public enum FILETYPE
    {
        MODEL,
        ROI
    }
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
        public int CurrentSelectRoiTemplate { get { return Convert.ToInt16(GetValue(CurrentSelectRoiTemplateProperty)); } set { SetValue(CurrentSelectRoiTemplateProperty, value); } }
        public DependencyProperty CurrentSelectRoiTemplateProperty = DependencyProperty.Register("CurrentSelectRoiTemplate", typeof(int), typeof(UC_CameraView));

        public void SetLever(int nLever)
        {
            throw new NotImplementedException();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDelay();
        }

        private async void LoadDelay()
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(1500).Wait();
                    bFirstLoaded = false;
                }
                Application.Current.Dispatcher.Invoke(()=> SetAttachCamWindow(Cb_Cameras.SelectedIndex, true));
            });
        }

        public void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !bFirstLoaded)
                SetAttachCamWindow(Cb_Cameras.SelectedIndex,true);
            else
            {
                SetAttachCamWindow(Cb_Cameras.SelectedIndex,false);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Messenger.Default.Send("","WindowSizeChanged");
        }

        private void SetAttachCamWindow(int nCamID,bool bAttach=true)
        {
            if (bAttach)
                Vision.Vision.Instance.AttachCamWIndow(nCamID, "CameraViewCam", Cam1.HalconWindow);
            else
                Vision.Vision.Instance.DetachCamWindow(nCamID, "CameraViewCam");
        }

        #region 动画
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard RoiSb = FindResource("RoiSb") as Storyboard;
            Storyboard TemplateSb = FindResource("TemplateSb") as Storyboard;
            CurrentSelectRoiTemplate = CurrentSelectRoiTemplate == 0 ? 1 : 0;
            if (CurrentSelectRoiTemplate == 0)
                TemplateSb.Begin();
            else
                RoiSb.Begin();
        }

        private void Storyboard2TemplateCompleted(object sender, EventArgs e)
        {
            ListBoxRoiTemplate.ItemsSource = (DataContext as MainWindowViewModel).TemplateCollection;
        }

        private void Storyboard2RoiCompleted(object sender, EventArgs e)
        {
            ListBoxRoiTemplate.ItemsSource = (DataContext as MainWindowViewModel).RoiCollection;
        }


        #endregion

        private void Cb_Cameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!bFirstLoaded)
                Vision.Vision.Instance.AttachCamWIndow(Cb_Cameras.SelectedIndex, "CameraViewCam", Cam1.HalconWindow);
        }
    }
}
