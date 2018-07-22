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
using CPAS.Instrument;
using System.IO;

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
        private int _maxThre = 0, _minThre=0;
        public PrescriptionGridModel _prescriptionUsed = new PrescriptionGridModel();
        private SystemParaModel _systemPataModelUsed = new SystemParaModel();
        private EnumCamSnapState _amSnapState;
        private int _currentSelectRoiTemplate=0;
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

        private void SetPrescriptionToPLC(PrescriptionGridModel prescription)
        {
            QSerisePlc PLC = InstrumentMgr.Instance.FindInstrumentByName("PLC") as QSerisePlc;
            if (PLC != null)
            {
                PLC.WriteInt("R12", prescription.EnableUnLock ? 1 : 2);
                PLC.WriteInt("R17", prescription.EnableReadBarcode ? 1 : 2);
                PLC.WriteInt("R65", prescription.EnableAdjustLaser ? 1 : 2);
                PLC.WriteInt("R106", prescription.EnableAdjustHoriz ? 1 : 2);
                PLC.WriteInt("R210", prescription.EnableAdjustFocus ? 1 : 2);
                PLC.WriteInt("R309", prescription.EnableCalibration ? 1 : 2);
            }
        }
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
        public int CurrentSelectRoiTemplate
        {
            set
            {
                if (_currentSelectRoiTemplate != value)
                {
                    _currentSelectRoiTemplate = value;
                    RaisePropertyChanged();
                }
            }
            get { return _currentSelectRoiTemplate; }
        }
        public int MaxThre
        {
            set
            {
                if (_maxThre != value)
                {
                    _maxThre = value;
                    RaisePropertyChanged();
                }
            }
            get { return _maxThre; }
        }
        public int MinThre
        {
            set
            {
                if (_minThre != value)
                {
                    _minThre = value;
                    RaisePropertyChanged();
                }
            }
            get { return _minThre; }
        }
        public Dictionary<string, WorkFlowBase> WorkeFlowDic
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
                SetPrescriptionToPLC(value);
                if (_prescriptionUsed != value)
                {
                    _prescriptionUsed = value;
                    SystemParaModelUsed = new SystemParaModel() {
                        BadBarcodeExpiration = SystemParaModelUsed.BadBarcodeExpiration,
                        CurPrescriptionName = value == null ? "" : value.Name,
                        ImageSaveExpiration= SystemParaModelUsed.ImageSaveExpiration
                    };
                    RaisePropertyChanged();

                }
            }
            get { return _prescriptionUsed; }
        }
        public SystemParaModel SystemParaModelUsed
        {
            get { return _systemPataModelUsed; }
            set
            {
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
        private void UpdateRoiCollect(int nCamID)
        {
            RoiCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetRoiListForSpecCamera(nCamID, RoiFileHelper.GetWorkDictoryProfileList(new string[] { "reg"})))
                RoiCollection.Add(new RoiItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
            LogHelper.WriteLine($"更新相机{nCamID}的ROI文件", LogHelper.LogType.NORMAL);
        }

        private void UpdateModelCollect(int nCamID)
        {
            TemplateCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, ModelFileHelper.GetWorkDictoryProfileList(new string[] { "shm" })))
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
        public RelayCommand<string> RibonCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    StrCurViewID = str;
                    LogHelper.WriteLine($"切换到{str}视图", LogHelper.LogType.NORMAL);
                });
            }
        }
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
        public RelayCommand LogOutCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Level = 0;
                    StrUserName = "Operator";
                    LogHelper.WriteLine($"注销登陆", LogHelper.LogType.NORMAL);
                });
            }
        }

        public RelayCommand<int> SnapOnceCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    Messenger.Default.Send<Tuple<string, int>>(new Tuple<string, int>("SnapOnce", nCamID), "SetCamState");
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
        public RelayCommand<Tuple<int, bool, HalconDotNet.HWindow>> SaveImagerCommand
        {
            get
            {
                return new RelayCommand<Tuple<int,bool,HalconDotNet.HWindow>>(para =>
                {
                    int nCamID = para.Item1;
                    bool bSaveImage = para.Item2;
                    HalconDotNet.HWindow hWindow = para.Item3;
                    DateTime now = DateTime.Now;
                    Vision.Vision.Instance.SaveImage(nCamID, bSaveImage ? EnumImageType.Image : EnumImageType.Window, FileHelper.GetCurFilePathString() + "ImageSaved\\ImageSaved", $"{now.Month}月{now.Day}日 {now.Hour}时{now.Minute}分{now.Second}秒_Cam{nCamID}.jpg", hWindow);
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
                        string strRemark = Window_AddNewPrescription.ProfileValue.Item2;
                        if (strName != "" && (from prescription in PrescriptionCollection where prescription.Name == strName select prescription).Count() == 0)
                        {
                            PrescriptionCollection.Add(new PrescriptionGridModel()
                            {
                                Name = strName,
                                Remark = strRemark,
                                EnableUnLock = true,
                                EnableReadBarcode = true,
                                EnableAdjustLaser = true,
                                EnableAdjustHoriz = true,
                                EnableAdjustFocus = true,
                                EnableCalibration = true,
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
                                    if (SystemParaModelUsed != null && SystemParaModelUsed.CurPrescriptionName == model.Name)
                                    {
                                        SystemParaModelUsed.CurPrescriptionName = "未选择配方";
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
                    {
                        PrescriptionUsed = model;
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.SystemParaCfg, new SystemParaModel[] { SystemParaModelUsed }); //自动保存系统参数
                    }
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
                        if (user.Password.Trim() == "")
                        {
                            UC_MessageBox.ShowMsgBox("密码不能为空", "提示");
                            return;
                        }
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.UserCfg, UserModelCollection.ToArray());
                        UC_MessageBox.ShowMsgBox("修改密码成功", "成功");
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
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.SystemParaCfg, new SystemParaModel[] { SystemParaModelUsed });
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


        //Roi Model
 
        public RelayCommand<int> NewRoiCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    if (nCamID >= 0)
                    {
                        if (MessageBoxResult.Yes == Window_AddRoiModel.ShowWindowNewRoiModel(EnumWindowType.ROI))
                        {
                            foreach (var it in RoiCollection)
                            {
                                if (it.StrName == Window_AddRoiModel.ProfileValue)
                                {
                                    UC_MessageBox.ShowMsgBox("该文件已经存在，请重新命名");
                                    return;
                                }
                            }
                            Vision.Vision.Instance.NewRoi(nCamID, $"VisionData\\Roi\\Cam{nCamID}_{Window_AddRoiModel.ProfileValue}");
                            //Vision.Vision.Instance.ShowRoi($"Cam{nCamID}_{Window_AddRoiModel.ProfileValue}");
                            UpdateRoiCollect(nCamID);   //只更新这一个相机的Roi文件
                        }
                    }
                });
            }
        }
        public RelayCommand<int> PreCreateModelCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    if (nCamID >= 0)
                    {
                        if (MessageBoxResult.Yes == Window_AddRoiModel.ShowWindowNewRoiModel(EnumWindowType.ROI))
                        {
                            foreach (var it in TemplateCollection)
                            {
                                if (it.StrName == Window_AddRoiModel.ProfileValue)
                                {
                                    UC_MessageBox.ShowMsgBox("该文件已经存在，请重新命名");
                                    return;
                                }
                            }
                            string strRegionTemp = $"VisionData\\ModelTemp\\Cam{nCamID}_{Window_AddRoiModel.ProfileValue}.reg";
                            FileHelper.DeleteAllFileInDirectory($"VisionData\\ModelTemp");
                            File.OpenWrite(strRegionTemp);
                            Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionTemp);
                            
                        }
                       
                    }
                });
            }
        }
        public RelayCommand<Tuple<RoiModelBase,int>> PreDrawModelRoiCommand     //调整Model的ROI
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    TemplateItem item = tuple.Item1 as TemplateItem;
                    int nCamID = tuple.Item2;
                    if (fileList.Count == 0)
                    {
                        if (item != null)      //判断编辑现有的还是编辑新模板
                        {
                            if (nCamID >= 0)
                            {
                                string regionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                                Vision.Vision.Instance.DrawRoi(nCamID, EnumRoiType.ModelRegionReduce, out object region, regionPath);    //有模板的时候直接以名称存储
                                Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, regionPath, region);
                            }
                        }
                    }
                    else
                    {
                        if (nCamID >= 0)
                        {
                            string regionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                            Vision.Vision.Instance.DrawRoi(nCamID, EnumRoiType.ModelRegionReduce, out object region, regionPath);       //没有模板的时候就按照相机存储
                            Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, regionPath, region);    //传入Region
                        }
                    }
                });
            }
        }
        public RelayCommand<Tuple<RoiModelBase, int>> PreViewRoiCommand     //只是动态显示，不绘图
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    TemplateItem item = tuple.Item1 as TemplateItem;
                    int nCamID = tuple.Item2;
                    if (fileList.Count == 0)
                    {
                        if (item != null)
                        {
                            if (nCamID >= 0)
                            {
                                string strRegionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                                object region = Vision.Vision.Instance.ReadRegion(strRegionPath);
                                Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);
                            }
                        }
                    }
                    else
                    {
                        if (nCamID >= 0)
                        {
                            string strRegionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                            object region = Vision.Vision.Instance.ReadRegion(strRegionPath);
                            Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);    //传入Region
                        }
                    }
                });
            }
        }

        public RelayCommand<RoiModelBase> ShowRoiModelCommand
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item =>
                {
                    if (item != null)
                    {
                        if(item .GetType()== typeof(RoiItem))
                            Vision.Vision.Instance.ShowRoi($"VisionData\\Roi\\{item.StrFullName}.reg");
                        else
                            Vision.Vision.Instance.ShowModel($"VisionData\\Model\\{item.StrFullName}.shm");
                    }
                });
            }
        }

        public RelayCommand<Tuple<RoiModelBase, int>> SaveModelParaCommand
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    TemplateItem item = tuple.Item1 as TemplateItem;
                    int nCamID = tuple.Item2;
                    if (fileList.Count == 0)
                    {
                        if (item != null)
                        {
                            if (nCamID >= 0)
                            {
                                string strRegionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                                object region = Vision.Vision.Instance.ReadRegion(strRegionPath);
                                Vision.Vision.Instance.SaveShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);
                                UpdateModelCollect(nCamID);   //只更新这一个相机的Roi文件
                            }
                        }
                       
                    }
                    else
                    {
                        if (nCamID >= 0)
                        {
                            string strRegionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                            object region = Vision.Vision.Instance.ReadRegion(strRegionPath);
                            FileHelper.DeleteAllFileInDirectory($"VisionData\\ModelTemp");  //删除Temp文件
                            Vision.Vision.Instance.SaveShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, $"VisionData\\Model\\{fileList[0]}.reg", region);    //传入Region
                            UpdateModelCollect(nCamID);   //只更新这一个相机的Roi文件
                        }
                    }
                });
            }
        }
        public RelayCommand<Tuple<RoiModelBase, int>> TestModelParaCommand
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    TemplateItem item = tuple.Item1 as TemplateItem;
                    int nCamID = tuple.Item2;
                    if (item != null && fileList.Count == 0)
                    {
                        if (nCamID >= 0)
                        {
                            string strRegionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                            object region = Vision.Vision.Instance.ReadRegion(strRegionPath);
                            Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);
                        }
                    }
                    else
                    {
                        string strRegionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                        object region = Vision.Vision.Instance.ReadRegion(strRegionPath);
                        Vision.Vision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);    //传入Region

                    }
                });
            }
        }
        public RelayCommand<string> TestRoiModelCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    string para = str.Split('&')[0];
                    int nCamID= Convert.ToInt16(str.Split('&')[1]);
                    if (nCamID < 0)
                        return;
                    switch (str)
                    {
                        case "Roi":
                            Vision.Vision.Instance.ProcessImage(Vision.Vision.IMAGEPROCESS_STEP.GET_ANGLE_TUNE1, nCamID, null, out object result);
                            break;
                        case "Model":

                            break;
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
            Messenger.Default.Register<int>(this, "UpdateRoiFiles", nCamID => UpdateRoiCollect(nCamID));
            Messenger.Default.Register<int>(this, "UpdateTemplateFiles", nCamID => UpdateModelCollect(nCamID));
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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SystemMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.ERROR, StrMsg = str });
                    });
                }
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
            Messenger.Default.Register<string>(this, "ShowPLCError", str =>
            {
                lock (SystemMessageCollection)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PLCMessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.ERROR, StrMsg = str });
                    });
                }
                LogHelper.WriteLine(str, LogHelper.LogType.ERROR);
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


            //Load Config
            PrescriptionCollection = new ObservableCollection<PrescriptionGridModel>();
            ConfigMgr.Instance.LoadConfig();
            foreach (var it in ConfigMgr.PrescriptionCfgMgr.Prescriptions)
                PrescriptionCollection.Add(it);


            //Init workflow state
            WorkeFlowDic = WorkFlowMgr.Instance.stationDic;

            //User config
            UserModelCollection = new ObservableCollection<UserModel>();
            foreach (var it in ConfigMgr.UserCfgMgr.Users)
            {
                UserModelCollection.Add(it);
            }


            //Camera
            int i = 0;
            foreach (var it in Vision.Vision.Instance.FindCamera(EnumCamType.HuaRay))
            {
                bool bOpen = Vision.Vision.Instance.OpenCam(i++);
                CameraCollection.Add(new CameraItem() { CameraName = it.Key, StrCameraState = bOpen ? "Connected" : "DisConnected" });
            }

            //当前选择的配方
            SystemParaModelUsed = ConfigMgr.SystemParaCfgMgr.SystemParaModels[0];
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