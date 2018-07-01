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
using CPSA.CLasses;
using System.Text;
using CPAS.Config.SoftwareManager;
using CPAS.WorkFlow;

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
        private object PlcErrLock = new object();
        private object SysErrLock = new object();

            

        private string _strViewID = "Home";
        private DateTime _myDateTime;
        private string _strUserName = "Operator";
        private int _level = 0;
        private string _strPLCErrorNumber = "", _strSystemErrorNumber = "";
        private bool _showPlcErrorListEdit;
        private string _strPowerMeterValue1 = "NA", _strPowerMeterValue2 = "NA";
        public PrescriptionGridModel _prescriptionUsed = new PrescriptionGridModel();
        private SystemParaModel _systemPataModelUsed = new SystemParaModel();
        private EnumCamSnapState _amSnapState;
        public Dictionary<string, WorkFlowBase> _workeFlowDic;
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
        private List<string> CamList = new List<string>() { "Cam1", "Cam2", "Cam3", "Cam4", "Cam5", "Cam6" };
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
        public Dictionary<string,WorkFlowBase> WorkeFlowDic
        {
            get { return _workeFlowDic; }
            set
            {
                if (_workeFlowDic != value)
                {
                    _workeFlowDic = value;
                    RaisePropertyChanged();
                }
            }
        }
        public PrescriptionGridModel PrescriptionUsed
        {
            set
            {
                if (_prescriptionUsed != value)
                {
                    _prescriptionUsed = value;
                    SystemPataModelUsed = new SystemParaModel() { BadBarcodeExpiration = SystemPataModelUsed.BadBarcodeExpiration, CurPrescriptionName = value == null ? "" : value.Name};
                    RaisePropertyChanged();
                }
            }
            get { return _prescriptionUsed; }
        }
        public SystemParaModel SystemPataModelUsed
        {
            get { return _systemPataModelUsed; }
            set {
                _systemPataModelUsed = value;
                RaisePropertyChanged();
                }
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
        public void ShowMessage(MessageItem msgItem)
        {
            lock (PlcErrLock)
            {
               PLCMessageCollection.Add(msgItem);
            }
  
        }
        private void UpdateRoiCollect(int nCamID)
        {
            RoiCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, RoiFileHelper.GetWorkDictoryProfileList()))
                RoiCollection.Add(new RoiItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
            LogHelper.WriteLine($"更新相机{nCamID}的ROI文件", LogHelper.LogType.NORMAL);
        }
        private void UpdateModelCollect(int nCamID)
        {
            TemplateCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, ModelFileHelper.GetWorkDictoryProfileList()))
                TemplateCollection.Add(new TemplateItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
            LogHelper.WriteLine($"更新相机{nCamID}的模板文件", LogHelper.LogType.NORMAL);
        }
        private void PLCMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (PlcErrLock)
            {
                var collect = from msg in PLCMessageCollection where msg.MsgType == MSGTYPE.ERROR select msg;
                if (collect.Count() != 0)
                    StrPLCErrorNumber = string.Format("{0}", collect.Count());
                else
                    StrPLCErrorNumber = "";
                if (PLCMessageCollection.Count > 50)
                    PLCMessageCollection.RemoveAt(0);
            }
        }
        private void SystemMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (SysErrLock)
            {
                var collect = from msg in SystemMessageCollection where msg.MsgType == MSGTYPE.ERROR select msg;
                if (collect.Count() != 0)
                    StrSystemErrorNumber = string.Format("{0}", collect.Count());
                else
                    StrSystemErrorNumber = "";
                if (SystemMessageCollection.Count > 50)
                    SystemMessageCollection.RemoveAt(0);
            }
        }
        #endregion


        #region Commands
        public RelayCommand<string> RibonCommand { get {
                return new RelayCommand<string>(str => { StrCurViewID = str;
                    LogHelper.WriteLine($"切换到{str}视图", LogHelper.LogType.NORMAL);
                });
            } }
        public RelayCommand<string> ClearMessageCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    switch (str)
                    {
                        case "ClearPLCMessage":
                            lock (PlcErrLock)
                            {
                                PLCMessageCollection.Clear();
                            }
                            LogHelper.WriteLine($"清除PLC错误信息记录", LogHelper.LogType.NORMAL);
                            break;
                        case "ClearSystemMessage":
                            lock (SysErrLock)
                            {
                                SystemMessageCollection.Clear();
                            }
                            LogHelper.WriteLine($"清除System错误信息记录", LogHelper.LogType.NORMAL);
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
                    LogHelper.WriteLine($"开始运行按钮被按下", LogHelper.LogType.NORMAL);
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
                    LogHelper.WriteLine($"结束运行按钮被按下", LogHelper.LogType.NORMAL);
                });
            }
        }
        public RelayCommand<Tuple<string, string>> LogInCommand
        {
            get
            {
                return new RelayCommand<Tuple<string, string>>(t =>
                {
                    bool bSuccess = false;
                    Tuple<string, string> tuple = t as Tuple<string, string>;
                    foreach (var it in ConfigMgr.UserCfgMgr.Users)
                    {
                        if (it.User == tuple.Item1 && it.Password == tuple.Item2)
                        {
                            Level = it.Level;
                            StrUserName = it.User;
                            bSuccess = true;
                            LogHelper.WriteLine($"用户{tuple.Item1}登陆成功", LogHelper.LogType.NORMAL);
                        }
                    }
                    if (!bSuccess)
                    {
                        LogHelper.WriteLine($"用户{tuple.Item1}登陆失败", LogHelper.LogType.NORMAL);

                    }
                });
            }
        }
        public RelayCommand LogOutCommand { get { return new RelayCommand(() => {
            Level = 0;
            StrUserName = "Operator";
            LogHelper.WriteLine($"注销登陆", LogHelper.LogType.NORMAL);
        }); } }

        public RelayCommand<int> SnapOnceCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    Messenger.Default.Send<Tuple<string,int>>(new Tuple<string, int>("SnapOnce",nCamID), "SetCamState");
                    CamSnapState = EnumCamSnapState.IDLE;
                    LogHelper.WriteLine($"单帧采集相机{nCamID}", LogHelper.LogType.NORMAL);
                });
            }
        }
        public RelayCommand<int> SnapContinuousCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    Messenger.Default.Send<Tuple<string, int>>(new Tuple<string, int>("SnapContinuous", nCamID), "SetCamState");
                    CamSnapState = EnumCamSnapState.BUSY;
                    LogHelper.WriteLine($"点击连续采集相机{nCamID}", LogHelper.LogType.NORMAL);
                });
            }
        }
        public RelayCommand<int> StopSnapCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    Messenger.Default.Send<Tuple<string, int>>(new Tuple<string, int>("StopSnap", nCamID), "SetCamState");
                    CamSnapState = EnumCamSnapState.IDLE;
                    LogHelper.WriteLine($"停止采集相机{nCamID}", LogHelper.LogType.NORMAL);
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
                    if (MessageBoxResult.Yes == Window_AddNewPrescription.ShowWindowNewDescription())
                    {
                        string strName = Window_AddNewPrescription.ProfileValue.Item1;
                        string strRemark= Window_AddNewPrescription.ProfileValue.Item2;
                        if (strName != "" && (from prescription in PrescriptionCollection where prescription.Name == strName select prescription).Count() == 0)
                        {
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
                            LogHelper.WriteLine($"新增配方{strName}:{strRemark}", LogHelper.LogType.NORMAL);
                        }
                        else if (strName == "")
                            UC_MessageBox.ShowMsgBox("名称不能为空");
                        else if ((from prescription in PrescriptionCollection where prescription.Name == strName select prescription).Count() != 0)
                            UC_MessageBox.ShowMsgBox("已经存此配方名称，请更换命名");
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
                        SaveSystemCfgCommand.Execute(null); //也要一起保存,这个有对话框，所以这个省掉不加
                        //UC_MessageBox.ShowMsgBox("保存成功", "提示");
                        LogHelper.WriteLine($"保存配方文件成功", LogHelper.LogType.NORMAL);
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox(ex.Message);
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
                                if (MessageBoxResult.Yes == UC_MessageBox.ShowMsgBox(string.Format("是否删除 {0} ?", model.Name)))
                                {
                                    bExist = true;
                                    PrescriptionCollection.Remove(model);
                                    if (SystemPataModelUsed != null && SystemPataModelUsed.CurPrescriptionName == model.Name)
                                    {
                                        SystemPataModelUsed.CurPrescriptionName = "未选择配方";
                                    }
                                    LogHelper.WriteLine($"删除配方{model.Name}:{model.Remark}", LogHelper.LogType.NORMAL);
                                    break;
                                }
                            }
                        }
                    }
                    if (!bExist)
                        UC_MessageBox.ShowMsgBox(string.Format("当前没有选中要删除的配方"));

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
                        UC_MessageBox.ShowMsgBox(string.Format("当前没有选中要使用的配方"));
                    else
                        PrescriptionUsed = model;
                    LogHelper.WriteLine($"设置使用{model.Name}:{model.Remark}为当前配方", LogHelper.LogType.NORMAL);
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
                        UC_MessageBox.ShowMsgBox("修改密码成功","成功");
                        LogHelper.WriteLine($"设置用户密码并保存成功", LogHelper.LogType.NORMAL);
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox(ex.Message);
                    }
                });
            }

        }
        public RelayCommand SaveSystemCfgCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.SystemParaCfg,new SystemParaModel[] { SystemPataModelUsed });
                        UC_MessageBox.ShowMsgBox("保存成功", "提示");
                        LogHelper.WriteLine($"保存配方文件成功", LogHelper.LogType.NORMAL);
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox(ex.Message);
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
                    //不需要加锁
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
                lock (SystemMessageCollection)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.ERROR, StrMsg = str });                      
                    });}
                    LogHelper.WriteLine(str, LogHelper.LogType.ERROR);
            });
            Messenger.Default.Register<string>(this, "ShowInfo", str =>
            {
                Application.Current.Dispatcher.Invoke(() => SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.INFO, StrMsg = str }));
                LogHelper.WriteLine(str, LogHelper.LogType.NORMAL);
            });
            Messenger.Default.Register<string>(this, "ShowWarning", str =>
            {
                Application.Current.Dispatcher.Invoke(() => SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.WARNING, StrMsg = str }));
                LogHelper.WriteLine(str, LogHelper.LogType.NORMAL);
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

            #region Log init
            LogHelper.LogEnabled = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\n\r\n");
            sb.Append("> =================================================================\r\n");
            sb.Append("> =                 LDS System                                    =\r\n");
            sb.Append("> =                Copyright (C)                                  =\r\n");
            sb.Append("> =================================================================\r\n\r\n");
            sb.Append(">-------------------------启动系统---------------------------------\r\n");
            LogHelper.WriteLine(sb.ToString());
            #endregion


            //Camera
            int i = 0;
            foreach(var it in CamList)
            {
                bool bOpen = Vision.Vision.Instance.OpenCam(i++);
                CameraCollection.Add(new CameraItem() { CameraName = it, StrCameraState = bOpen? "Connected" : "DisConnected" });
            }
            
            //Load Config
            PrescriptionCollection = new ObservableCollection<PrescriptionGridModel>();
            ConfigMgr.Instance.LoadConfig();
            foreach (var it in ConfigMgr.PrescriptionCfgMgr.Prescriptions)
                PrescriptionCollection.Add(it);


            //Init workflow state
            WorkeFlowDic= WorkFlowMgr.Instance.stationDic;

            //User config
            UserModelCollection = new ObservableCollection<UserModel>();
            foreach (var it in ConfigMgr.UserCfgMgr.Users)
            {
                UserModelCollection.Add(it);
            }

            //当前选择的配方
            SystemPataModelUsed = ConfigMgr.SystemParaCfgMgr.SystemParaModels[0];
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