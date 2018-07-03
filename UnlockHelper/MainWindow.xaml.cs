using System.Windows;
using UnlockHelper.ViewModel;
using UnlockHelper.Model;
namespace UnlockHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            string[] resultArray = new string[] { };
            UnlockHelper.Model.UnlockHelper.LdsUnlock(ref resultArray);
     
        }
    }
}