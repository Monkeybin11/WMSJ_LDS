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
using CPAS.UserCtrl;
using CPAS.Views;
using CPAS.Config.UserManager;

namespace CPAS.ViewModels
{
    public enum USER
    {
        OPERATOR,
        ENGINNER,
        MANAGER
    }

    public class MainWindowViewModel : ViewModelBase
    {

        #region Fields
        private string _strViewID = "Home";
        private DateTime _myDateTime;
        private string _strUserName = "Operator";
        private int _level = 0;
        private string _strPLCErrorNumber = "", _strSystemErrorNumber = "";
        private bool _showPlcErrorListEdit;
        private string _strPowerMeterValue1 = "NA", _strPowerMeterValue2 = "NA";
        public PrescriptionGridModel _prescriptionUsed = new PrescriptionGridModel();
        private EnumCamSnapState _amSnapState;
        private ObservableCollection<MessageItem> _plcMessageCollection = new ObservableCollection<MessageItem>();
        private ObservableCollection<MessageItem> _systemMessageCollection = new ObservableCollection<MessageItem>();
        private ObservableCollection<CameraItem> _cameraCollection = new ObservableCollection<CameraItem>();
        private ObservableCollection<RoiItem> _roiCollection = new ObservableCollection<RoiItem>();
        private ObservableCollection<TemplateItem> _templateCollection = new ObservableCollection<TemplateItem>();
        private Dictionary<string, string> LogInfoDic = new Dictionary<string, string>();
        private FileHelper ModelFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Model");
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
                if (_strUserName != value)
                {
                    _strUserName = value;
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
        public bool ShowPlcErrorListEdit
        {
            set
            {
                if (_showPlcErrorListEdit != value)
                {
                    _showPlcErrorListEdit = value;
                    RaisePropertyChanged();
                }
            }
            get { return _showPlcErrorListEdit; }
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
            get { return _amSnapState; }
        }
        public string StrPowerMeterValue1
        {
            set
            {
                if (_strPowerMeterValue1 != value)
                {
                    _strPowerMeterValue1 = value;
                    RaisePropertyChanged();
                }
            }
            get { return _strPowerMeterValue1; }
        }
        public string StrPowerMeterValue2
        {
            set
            {
                if (_strPowerMeterValue2 != value)
                {
                    _strPowerMeterValue2 = value;
                    RaisePropertyChanged();
                }
            }
            get { return _strPowerMeterValue2; }
        }
        public PrescriptionGridModel PrescriptionUsed
        {
            set
            {
                if (_prescriptionUsed != value)
                {
                    _prescriptionUsed = value.Clone() as PrescriptionGridModel;
                    RaisePropertyChanged();
                }
            }
            get { return _prescriptionUsed; }
        }
        public ObservableCollection<MessageItem> PLCMessageCollection
        {
            get { return _plcMessageCollection; }
            set
            {
                if (_plcMessageCollection != value)
                {
                    _plcMessageCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<MessageItem> SystemMessageCollection
        {
            get { return _systemMessageCollection; }
            set
            {
                if (_systemMessageCollection != value)
                {
                    _systemMessageCollection = value;
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
        public ObservableCollection<string>[] StepCollection { get; set; }
        public ObservableCollection<PrescriptionGridModel> PrescriptionCollection { get; set; }
        public ObservableCollection<UserModel> UserModelCollection { get; set; }
        #endregion


        #region  Method
        private void ShowMessage(MessageItem msgItem)
        {
            if (PLCMessageCollection.Count > 100)
                PLCMessageCollection.RemoveAt(0);
            if (msgItem != null)
                PLCMessageCollection.Add(msgItem);
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
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, ModelFileHelper.GetWorkDictoryProfileList()))
                TemplateCollection.Add(new TemplateItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
        }
        private void PLCMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var collect = from msg in PLCMessageCollection where msg.MsgType == MSGTYPE.ERROR select msg;
            if (collect.Count() != 0)
                StrPLCErrorNumber = string.Format("{0}", collect.Count());
            else
                StrPLCErrorNumber = "";
            if (PLCMessageCollection.Count > 50)
                PLCMessageCollection.RemoveAt(0);
        }
        private void SystemMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var collect = from msg in SystemMessageCollection where msg.MsgType == MSGTYPE.ERROR select msg;
            if (collect.Count() != 0)
                StrSystemErrorNumber = string.Format("{0}", collect.Count());
            else
                StrSystemErrorNumber = "";
            if (SystemMessageCollection.Count > 50)
                SystemMessageCollection.RemoveAt(0);
        }
        #endregion


        #region Commands
        public RelayCommand<string> RibonCommand { get { return new RelayCommand<string>(str => StrCurViewID = str); } }
        public RelayCommand<string> ClearMessageCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    switch (str)
                    {
                        case "ClearPLCMessage":
                            PLCMessageCollection.Clear();
                            break;
                        case "ClearSystemMessage":
                            SystemMessageCollection.Clear();
                            break;
                        default:
                            break;
                    }
                });
            }

        }
        public RelayCommand StartCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    WorkFlow.WorkFlowMgr.Instance.StartAllStation();
                });
            }
        }
        public RelayCommand StopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    WorkFlow.WorkFlowMgr.Instance.StopAllStation();

                });
            }
        }
        public RelayCommand<Tuple<string, string>> LogInCommand
        {
            get
            {
                return new RelayCommand<Tuple<string, string>>(t =>
                {
                    Tuple<string, string> tuple = t as Tuple<string, string>;
                    foreach (var it in ConfigMgr.UserCfgMgr.Users)
                    {
                        if (it.User == tuple.Item1 && it.Password == tuple.Item2)
                        {
                            Level = it.Level;
                            StrUserName = it.User;
                        }
                    }

                });
            }
        }
        public RelayCommand LogOutCommand { get { return new RelayCommand(() => { Level = 0; StrUserName = "Operator"; }); } }
        public RelayCommand<string> SetSnapStateCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    Messenger.Default.Send<string>(str, "SetCamState");
                    if (str.ToLower() == "snapcontinues")
                        CamSnapState = EnumCamSnapState.BUSY;
                    else
                        CamSnapState = EnumCamSnapState.IDLE;
                });
            }
        }
        public RelayCommand<int> UpdateRoiTemplate
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    UpdateModelCollect(nCamID);
                    UpdateRoiCollect(nCamID);
                });
            }
        }
        public RelayCommand<string> ShowErrorListEditCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    switch (str)
                    {
                        case "PLC":
                            ShowPlcErrorListEdit = true;
                            break;
                        case "Sys":
                            ShowPlcErrorListEdit = false;
                            break;
                        default:
                            break;
                    }
                });
            }
        }
        
        public RelayCommand AddPrescriptionCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxResult.Yes == Window_AddNewPrescription.Instance.ShowWindowNewDescription())
                    {
                        string strName = Window_AddNewPrescription.Instance.ProfileValue.Item1;
                        string strRemark= Window_AddNewPrescription.Instance.ProfileValue.Item2;
                        if (strName != "" && (from prescription in PrescriptionCollection where prescription.Name == strName select prescription).Count() == 0)
                            PrescriptionCollection.Add(new PrescriptionGridModel()
                            {
                                Name = strName,
                                Remark = strRemark,
                                UnLock = true,
                                ReadBarcode = true,
                                AdjustLaser = true,
                                AdjustHoriz = true,
                                AdjustFocus = true,
                                Calibration = true,
                            });
                        else if(strName=="")
                            UC_MessageBox.Instance.ShowMsgBox("名称不能为空");
                        else if((from prescription in PrescriptionCollection where prescription.Name == strName select prescription).Count()!=0)
                            UC_MessageBox.Instance.ShowMsgBox("已经存此配方名称，请更换命名");
                    }
                });
            }
        }
        public RelayCommand SavePrescriptionCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.PrescriptionCfg, PrescriptionCollection.ToArray());
                        UC_MessageBox.Instance.ShowMsgBox("保存成功", "提示");
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.Instance.ShowMsgBox(ex.Message);
                    }
                });
            }
        }
        public RelayCommand<PrescriptionGridModel> DeletePrescriptionCommand
        {
            get
            {
                return new RelayCommand<PrescriptionGridModel>(model =>
                {
                    bool bExist = false;
                    if (model != null)
                    {
                        foreach (var it in PrescriptionCollection)
                        {
                            if (it.Name == model.Name)
                            {
                                if (MessageBoxResult.Yes == UC_MessageBox.Instance.ShowMsgBox(string.Format("是否删除 {0} ?", model.Name)))
                                {
                                    bExist = true;
                                    PrescriptionCollection.Remove(model);
                                    break;
                                }
                            }
                        }
                    }
                    if (!bExist)
                        UC_MessageBox.Instance.ShowMsgBox(string.Format("当前没有选中要删除的配方"));

                });
            }
        }
        public RelayCommand<PrescriptionGridModel> SetPrescriptionUsedCommand
        {
            get
            {
                return new RelayCommand<PrescriptionGridModel>(model =>
                {
                    if (model == null)
                        UC_MessageBox.Instance.ShowMsgBox(string.Format("当前没有选中要使用的配方"));
                    else
                        PrescriptionUsed = model;
                });
            }
        }
        public RelayCommand<UserModel> SaveUserCfgCommand
        {
            get
            {
                return new RelayCommand<UserModel>(user =>
                {
                    try
                    {
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.UserCfg, UserModelCollection.ToArray());
                        UC_MessageBox.Instance.ShowMsgBox("修改密码成功","成功");
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.Instance.ShowMsgBox(ex.Message);
                    }
                });
            }

        }
        #endregion

        #region Ctor and DeCtor
        public MainWindowViewModel()
        {
            PLCMessageCollection.CollectionChanged += PLCMessageCollection_CollectionChanged;
            SystemMessageCollection.CollectionChanged += SystemMessageCollection_CollectionChanged;

            #region Messages
            Messenger.Default.Register<string>(this, "UpdateRoiFiles", str => UpdateRoiCollect(Convert.ToInt16(str.Substring(3, 1))));
            Messenger.Default.Register<string>(this, "UpdateTemplateFiles", str => UpdateModelCollect(Convert.ToInt16(str.Substring(3, 1))));
            Messenger.Default.Register<Tuple<string, string>>(this, "ShowStepInfo", tuple =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (tuple.Item1)
                        {
                            case "WorkRecord":
                                StepCollection[0].Add(tuple.Item2);
                                if (StepCollection[0].Count > 50)
                                    StepCollection[0].RemoveAt(0);
                                break;
                            case "WorkTune1":
                                StepCollection[1].Add(tuple.Item2);
                                if (StepCollection[1].Count > 50)
                                    StepCollection[1].RemoveAt(0);
                                break;
                            case "WorkTune2":
                                StepCollection[2].Add(tuple.Item2);
                                if (StepCollection[2].Count > 50)
                                    StepCollection[2].RemoveAt(0);
                                break;
                            case "WorkCalib":
                                StepCollection[3].Add(tuple.Item2);
                                if (StepCollection[3].Count > 50)
                                    StepCollection[3].RemoveAt(0);
                                break;
                            default:
                                break;
                        }
                    });

                });

            Messenger.Default.Register<string>(this, "ShowError", str =>
            {
                Application.Current.Dispatcher.Invoke(() => SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.ERROR, StrMsg = str }));
            });
            Messenger.Default.Register<string>(this, "ShowInfo", str =>
            {
                Application.Current.Dispatcher.Invoke(() => SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.INFO, StrMsg = str }));
            });
            Messenger.Default.Register<string>(this, "ShowWarning", str =>
            {
                Application.Current.Dispatcher.Invoke(() => SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.WARNING, StrMsg = str }));
            });
            Messenger.Default.Register<Tuple<string, string, string>>(this, "WorkFlowMessage", tuple =>
            {
                string strWorkFlowName = tuple.Item1;
                string strMethodName = tuple.Item2;
                string strPara = tuple.Item3;
                switch (strWorkFlowName)
                {
                    case "WorkRecord":
                        switch (strMethodName)
                        {
                            case "ShowPower":
                                StrPowerMeterValue1 = strPara.Split(',')[0];
                                StrPowerMeterValue2 = strPara.Split(',')[1];
                                break;
                        }
                        break;
                }
            });

            #endregion

            //StepInfo  init
            StepCollection = new ObservableCollection<string>[]{
                    new ObservableCollection<string>(),
                    new ObservableCollection<string>(),
                    new ObservableCollection<string>(),
                    new ObservableCollection<string>()
                };

            //Roi Model
            UpdateModelCollect(0);
            UpdateRoiCollect(0);

            //Camera
            CameraCollection.Add(new CameraItem() { CameraName = "CameraView_Cam1", StrCameraState = "Connected" });
            CameraCollection.Add(new CameraItem() { CameraName = "CameraView_Cam2", StrCameraState = "Disconnected" });
            CameraCollection.Add(new CameraItem() { CameraName = "CameraView_Cam3", StrCameraState = "Connected" });

            //Load Config
            PrescriptionCollection = new ObservableCollection<PrescriptionGridModel>();
            ConfigMgr.Instance.LoadConfig();
            foreach (var it in ConfigMgr.PrescriptionCfgMgr.Prescriptions)
                PrescriptionCollection.Add(it);

            //User config
            UserModelCollection = new ObservableCollection<UserModel>();
            foreach (var it in ConfigMgr.UserCfgMgr.Users)
            {
                UserModelCollection.Add(it);
            }

        }
        ~MainWindowViewModel()
        {
            // Unregister
            Messenger.Default.Unregister<string>("ShowError");
            Messenger.Default.Unregister<string>("ShowInfo");
            Messenger.Default.Unregister<string>("ShowWarning");
            Messenger.Default.Unregister<string>("UpdateRoiFiles");
            Messenger.Default.Unregister<string>("UpdateTemplateFiles");
            Messenger.Default.Unregister<Tuple<string, string>>("ShowStepInfo");
            Messenger.Default.Unregister<Tuple<string, string, string>>("WorkFlowMessage");
        }
        #endregion
    }
}