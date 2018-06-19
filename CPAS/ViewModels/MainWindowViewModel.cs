using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using CPAS.Interface;
using CPAS.Classes;
using CPAS.Config;
using CPAS.Vision;

namespace CPAS.ViewModels
{
    public enum USER
    {
        OPERATOR,
        ENGINNER,
        MANAGER
    }
    public enum EnumCamSnapState
    {
        IDLE,
        BUSY,
        DISCONNECTED
    }
    public class MainWindowViewModel : ViewModelBase
    {
    
        #region Fields
        private string _strViewID = "Home";
        private DateTime _myDateTime;
        private string _strUserName="";
        private int _level;
        private string _strPLCErrorNumber, _strSystemErrorNumber;
        private EnumCamSnapState _amSnapState;
        private ObservableCollection<MessageItem> _messageCollection=new ObservableCollection<MessageItem>();
        private ObservableCollection<CameraItem> _cameraCollection = new ObservableCollection<CameraItem>();
        private ObservableCollection<RoiItem> _roiCollection = new ObservableCollection<RoiItem>();
        private ObservableCollection<TemplateItem> _templateCollection = new ObservableCollection<TemplateItem>();
        private Dictionary<string, string> LogInfoDic = new Dictionary<string, string>();
        private FileHelper ModelFileHelper = new FileHelper(FileHelper.GetCurFilePathString()+"VisionData\\Model");
        private FileHelper RoiFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Roi");
        private Dictionary<int, string> ModelNameDic = new Dictionary<int, string>();
        private Dictionary<int, string> RoiNameDic = new Dictionary<int, string>();
        #endregion


        #region Property
        public DateTime MyDateTime
        {
            get { return _myDateTime; }
            set
            {
                if (_myDateTime != value)
                {
                    _myDateTime = value;
                    RaisePropertyChanged(() => MyDateTime);
                }
            }
        }
        public string StrCurViewID
        {
            set
            {
                if (_strViewID != null && _strViewID != value)
                {
                    _strViewID = value;
                    RaisePropertyChanged(() => StrCurViewID);
                }
            }
            get { return _strViewID; }
        }
        public string StrUserName
        {
            set
            {
                if (_strUserName.Replace("user: ", "") != value)
                {
                    _strUserName = "user: " + value;
                    RaisePropertyChanged();
                }
            }
            get { return _strUserName; }
        }
        public string StrPLCErrorNumber
        {
            get
            {
                return _strPLCErrorNumber;
            }
            set
            {
                if (_strPLCErrorNumber != value)
                {
                    _strPLCErrorNumber = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string StrSystemErrorNumber
        {
            get
            {
                return _strSystemErrorNumber;
            }
            set
            {
                if (_strSystemErrorNumber != value)
                {
                    _strSystemErrorNumber = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Level
        {
            set
            {
                if (_level != value)
                {
                    _level = value;
                    RaisePropertyChanged();
                }
            }
            get { return _level; }
        }
        public Enum_REGION_TYPE RegionType
        {
            get { return Vision.Vision.Instance.RegionType; }
            set { Vision.Vision.Instance.RegionType = value; }
        }
        public Enum_REGION_OPERATOR RegionOperator
        {
            get { return Vision.Vision.Instance.RegionOperator; }
            set { Vision.Vision.Instance.RegionOperator = value; }
        }
        public EnumCamSnapState CamSnapState
        {
            set
            {
                if (_amSnapState != value)
                {
                    _amSnapState = value;
                    RaisePropertyChanged();
                }
            }
            get { return _amSnapState;}
        }
        public ObservableCollection<MessageItem> MessageCollection
        {
            get { return _messageCollection; }
            set
            {
                if (_messageCollection != value)
                {
                    _messageCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<CameraItem> CameraCollection
        {
            get { return _cameraCollection; }
            set
            {
                if (_cameraCollection != value)
                {
                    _cameraCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<RoiItem> RoiCollection
        {
            get { return _roiCollection; }
            set
            {
                if (_roiCollection != value)
                {
                    _roiCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<TemplateItem> TemplateCollection
        {
            get { return _templateCollection; }
            set
            {
                if (_templateCollection != value)
                {
                    _templateCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region  Method
        private void ShowMessage(MessageItem msgItem)
        {
            if (MessageCollection.Count > 100)
                MessageCollection.RemoveAt(0);
            if (msgItem != null)
                MessageCollection.Add(msgItem);
        }
        private void UpdateRoiCollect(int nCamID)
        {
            RoiCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, RoiFileHelper.GetWorkDictoryProfileList()))
                RoiCollection.Add(new RoiItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
        }
        private void UpdateModelCollect(int nCamID)
        {
            TemplateCollection.Clear();
            foreach(var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, ModelFileHelper.GetWorkDictoryProfileList()))
                TemplateCollection.Add(new TemplateItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""),StrFullName =it });
        }
        #endregion




        #region Commands
        public RelayCommand<string> RibonCommand { get { return new RelayCommand<string>(str => StrCurViewID = str); } }
        public RelayCommand<string> ShowInfoCommand
        {
            get
            {
                return new RelayCommand<string>(str => {
                    switch (str)
                    { 
                        case "Clear":
                            MessageCollection.Clear();
                            break;
                    }
                });
            } 
        
        }
        public RelayCommand StartCommand { get {
                return new RelayCommand(() => {
                   Messenger.Default.Send<string>("Start from UI", "ShowError");
                });
            } }
        public RelayCommand StopCommand
        {
            get
            {
                return new RelayCommand(() => {
                    Messenger.Default.Send<string>("Stop from UI", "ShowInfo");
                });
            }
        }
        public RelayCommand<Tuple<string,string>> LogInCommand { get { return new RelayCommand<Tuple<string,string>>(t=> {
            Tuple<string, string> tuple = t as Tuple<string, string>;
            string usr = tuple.Item1;
            string psd = tuple.Item2;
            if(LogInfoDic.Keys.Contains(usr))
            {
                if (psd == LogInfoDic[usr])
                {
                    StrUserName = usr;
                    switch (usr)
                    {
                        case "Operator":
                            Level = 0;
                            break;
                        case "Engineer":
                            Level = 1;
                            break;
                        case "Manager":
                            Level = 2;
                            break;
                        default:
                            break;
                    }
                }

            }
        }); } }
        public RelayCommand LogOutCommand { get { return new RelayCommand(() => { Level = 0; StrUserName = "Operator"; }); } }
        public RelayCommand<string> SetSnapStateCommand { get { return new RelayCommand<string>(
            str => {
                Messenger.Default.Send<string>(str, "SetCamState");
                if (str.ToLower() == "snapcontinues")
                    CamSnapState = EnumCamSnapState.BUSY;
                else
                    CamSnapState = EnumCamSnapState.IDLE;
            } ); } }
        public RelayCommand<int> UpdateRoiTemplate { get { return new RelayCommand<int>(nCamID=> {
            UpdateModelCollect(nCamID);
            UpdateRoiCollect(nCamID);
        }); } }
        
        #endregion

        #region Ctor and DeCtor
        public MainWindowViewModel()
        {
            _messageCollection.CollectionChanged += _messageCollection_CollectionChanged;
            #region Messages
            Messenger.Default.Register<string>(this, "UpdateRoiFiles", str =>UpdateRoiCollect(Convert.ToInt16(str.Substring(3,1))));
            Messenger.Default.Register<string>(this, "UpdateTemplateFiles", str => UpdateModelCollect(Convert.ToInt16(str.Substring(3, 1))));
            #endregion

            //User Manager
            Level = 0;
            LogInfoDic.Add("Operator","111");
            LogInfoDic.Add("Engineer", "222");
            LogInfoDic.Add("Manager", "333");

            //Roi Model
            UpdateModelCollect(0);
            UpdateRoiCollect(0);

            //Camera
            CameraCollection.Add(new CameraItem() { CameraName = "CameraView_Cam1", StrCameraState="Connected"});
            CameraCollection.Add(new CameraItem() { CameraName = "CameraView_Cam2" , StrCameraState = "Disconnected" });
            CameraCollection.Add(new CameraItem() { CameraName = "CameraView_Cam3" , StrCameraState = "Connected" });

            //Message
            Messenger.Default.Register<string>(this,"ShowError", str => {
                Application.Current.Dispatcher.Invoke(()=>MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.ERROR, StrMsg = str}));
            });
            Messenger.Default.Register<string>(this,"ShowInfo", str => {
                Application.Current.Dispatcher.Invoke(() => MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.INFO, StrMsg = str }));
            });
            Messenger.Default.Register<string>(this,"ShowWarning", str => {
                Application.Current.Dispatcher.Invoke(() => MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.WARNING, StrMsg = str }));
            });
            StrUserName = "Operator";

            //Load Config
            ConfigMgr.Instance.LoadConfig();
        }

        private void _messageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (MessageCollection.Count != 0)
                StrPLCErrorNumber = string.Format("PLC {0} error", MessageCollection.Count);
            else
                StrPLCErrorNumber = "PLC";
        }

        ~MainWindowViewModel()
        {
            Messenger.Default.Unregister<string>("ShowError");
            Messenger.Default.Unregister<string>("ShowInfo");
            Messenger.Default.Unregister<string>("ShowWarning");
        }
        #endregion

       
      
    }
}