using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;
using LDSFuncSet;

namespace CPAS.Vision
{
    public enum Enum_REGION_OPERATOR { ADD,SUB}
    public enum Enum_REGION_TYPE { RECTANGLE,CIRCLE}
    public enum EnumCamSnapState
    {
        IDLE,
        BUSY,
        DISCONNECTED
        
    }
    public enum EnumCamType
    {
        GigEVision,
        DirectShow,
        uEye,
        HuaRay
    }
    public enum EnumImageType
    {
        Window,
        Image
    }
    public enum EnumShapeModelType
    {
        Gray,
        Shape,
        XLD
    };
    public class Vision
    {
        #region constructor
        private Vision()
        {
            for (int i = 0; i < 10; i++)
            {
                HoImageList.Add(new HObject());
                AcqHandleList.Add(new HTuple());
                _lockList.Add(new object());
            }
            HOperatorSet.GenEmptyObj(out Region);
            HOperatorSet.GenEmptyObj(out ImageTemp);

            CamCfgDic = FindCamera(EnumCamType.HuaRay);
        }
        private static readonly Lazy<Vision> _instance = new Lazy<Vision>(() => new Vision());
        public static Vision Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region  var
        public List<object> _lockList = new List<object>();
        public enum IMAGEPROCESS_STEP
        {
            GET_ANGLE_TUNE1,
            GET_ANGLE_BLOB,
            GET_ANGLE_TUNE2,
        }
        private List<HObject> HoImageList = new List<HObject>(10);    //Image
        private List<HTuple> AcqHandleList = new List<HTuple>(10);    //Aqu
        private Dictionary<int, Dictionary<string, HTuple>> HwindowDic = new Dictionary<int, Dictionary<string, HTuple>>();    //Hwindow
        private Dictionary<int, Tuple<HTuple, HTuple>> ActiveCamDic = new Dictionary<int, Tuple<HTuple, HTuple>>();
        private Dictionary<string, Tuple<string, string>> CamCfgDic = new Dictionary<string, Tuple<string, string>>();
        private HObject Region = null;
        public Enum_REGION_OPERATOR RegionOperator = Enum_REGION_OPERATOR.ADD;
        public Enum_REGION_TYPE RegionType = Enum_REGION_TYPE.CIRCLE;
        private LDSVisionFunc LdsFuncSet = new LDSVisionFunc();
        private HObject ImageTemp = null;
        #endregion

        #region public method 
        public bool AttachCamWIndow(int nCamID, string Name, HTuple hWindow)
        {
            if (nCamID < 0)
                return false;
            lock (_lockList[nCamID])
            {

                //关联当前窗口
                if (HwindowDic.Keys.Contains(nCamID))
                {
                    var its = from hd in HwindowDic[nCamID] where hd.Key == Name select hd;
                    if (its.Count() == 0)
                        HwindowDic[nCamID].Add(Name, hWindow);
                    else
                        HwindowDic[nCamID][Name] = hWindow;
                }
                else
                    HwindowDic.Add(nCamID, new Dictionary<string, HTuple>() { { Name, hWindow } });
                if (ActiveCamDic.Keys.Contains(nCamID))
                    HOperatorSet.SetPart(HwindowDic[nCamID][Name], 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);


                //需要解除此窗口与其他相机的关联
                foreach (var kps in HwindowDic)
                {
                    if (kps.Key == nCamID)
                        continue;
                    foreach (var kp in kps.Value)
                    {
                        if (kp.Key == Name)
                        {
                            kps.Value.Remove(Name);
                            break;
                        }
                    }
                }
                return true;
            }

        }
        public bool DetachCamWindow(int nCamID, string Name)
        {
            if (nCamID < 0)
                return false;
            lock (_lockList[nCamID])
            {
                if (HwindowDic.Keys.Contains(nCamID))
                {
                    if (HwindowDic[nCamID].Keys.Contains(Name))
                        HwindowDic[nCamID].Remove(Name);
                }
                return true;
            }
        }
        public bool OpenCam(int nCamID)
        {
            if (nCamID < 0)
                return false;
            HObject image = null;
            HTuple hv_AcqHandle = null;
            HTuple width = 0, height = 0;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!IsCamOpen(nCamID))
                    {
                        //HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                        //                            -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
                        HOperatorSet.OpenFramegrabber(CamCfgDic.ElementAt(nCamID).Value.Item2, 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                                                   -1, "false", "default", CamCfgDic.ElementAt(nCamID).Value.Item1, 0, -1, out hv_AcqHandle);
                        HOperatorSet.GrabImage(out image, hv_AcqHandle);
                        HOperatorSet.GetImageSize(image, out width, out height);
                        ActiveCamDic.Add(nCamID, new Tuple<HTuple, HTuple>(width, height));
                        AcqHandleList[nCamID] = hv_AcqHandle;
                    }
                    if (IsCamOpen(nCamID))
                    {
                        if (HwindowDic.Keys.Contains(nCamID))
                        {
                            foreach (var it in HwindowDic[nCamID])
                            {
                                HOperatorSet.SetPart(it.Value, 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);
                                HOperatorSet.DispObj(image, it.Value);
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>($"Open Camera Error:{CamCfgDic.ElementAt(nCamID)}:{ex.Message}","ShowError");
                return false;
            }
            finally
            {
                if (image != null)
                    image.Dispose();
            }
        }
        public bool CloseCam(int nCamID)
        {
            if (nCamID < 0)
                return false;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (ActiveCamDic.Keys.Contains(nCamID))
                    {
                        HOperatorSet.CloseFramegrabber(AcqHandleList[nCamID]);
                        ActiveCamDic.Remove(nCamID);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
                return false;
            }
        }
        public bool IsCamOpen(int nCamID)
        {
            if (nCamID < 0)
                return false;
            lock (_lockList[nCamID])
            {
                return ActiveCamDic.Keys.Contains(nCamID);
            }
        }
        public void GrabImage(int nCamID, bool bDispose = true)
        {
            if (nCamID < 0)
                return;
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!HwindowDic.Keys.Contains(nCamID))
                    {
                        Messenger.Default.Send<string>(string.Format("请先给相机{0}绑定视觉窗口", nCamID), "ShowError");
                        return;
                    }
                    if (!IsCamOpen(nCamID))
                        OpenCam(nCamID);
                    if (!IsCamOpen(nCamID))
                        return;
                    if (ImageTemp != null)
                    {
                        ImageTemp.Dispose();
                        ImageTemp = null;
                    }
                    HOperatorSet.GrabImage(out image, AcqHandleList[nCamID]);
                    HOperatorSet.GenEmptyObj(out Region);
                    HOperatorSet.GenEmptyObj(out ImageTemp);
                    HOperatorSet.ConcatObj(ImageTemp, image, out ImageTemp);
                    foreach (var it in HwindowDic[nCamID])
                        if (it.Value != -1)
                            HOperatorSet.DispObj(image, it.Value);
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
            }
            finally
            {
                if (bDispose && image != null)
                {

                    image.Dispose();
                }
            }
        }
        public bool ProcessImage(IMAGEPROCESS_STEP nStep, int nCamID, object para, out object result)
        {
            result = null;
            if (nCamID < 0)
                return false;
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    switch (nStep)
                    {
                        case IMAGEPROCESS_STEP.GET_ANGLE_TUNE1:

                            break;
                        case IMAGEPROCESS_STEP.GET_ANGLE_TUNE2:
                            break;
                        case IMAGEPROCESS_STEP.GET_ANGLE_BLOB:
                            break;
                        default:
                            break;
                    }
                    result = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                result = null;
                Messenger.Default.Send<string>(ex.Message, "Error");
                return false;
            }
            finally
            {
                image.Dispose();
            }
        }
        public bool DrawRoi(int nCamID)
        {
            if (nCamID < 0)
                return false;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains("CameraViewCam"))
                    {
                        HTuple window = HwindowDic[nCamID]["CameraViewCam"];
                        HTuple row, column, phi, length1, length2, radius;
                        HObject newRegion = null;
                        HOperatorSet.SetColor(window, "green");
                        switch (RegionType)
                        {
                            case Enum_REGION_TYPE.RECTANGLE:
                                HOperatorSet.DrawRectangle2(window, out row, out column, out phi, out length1, out length2);
                                HOperatorSet.GenRectangle2(out newRegion, row, column, phi, length1, length2);
                                break;
                            case Enum_REGION_TYPE.CIRCLE:
                                HOperatorSet.DrawCircle(window, out row, out column, out radius);
                                HOperatorSet.GenCircle(out newRegion, row, column, radius);
                                break;
                            default:
                                break;
                        }
                        if (RegionOperator == Enum_REGION_OPERATOR.ADD)
                        {
                            HOperatorSet.Union2(Region, newRegion, out Region);
                        }
                        else
                        {
                            HOperatorSet.Difference(Region, newRegion, out Region);
                        }

                        HOperatorSet.SetDraw(window, "fill");
                        HOperatorSet.SetColor(window, "red");
                        HOperatorSet.ClearWindow(window);
                        HOperatorSet.DispObj(ImageTemp, window);
                        HOperatorSet.DispObj(Region, window);
                        return true;
                    }
                    Messenger.Default.Send<String>("绘制模板窗口没有打开,或者该相机未关联绘制模板窗口", "ShowError");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<String>(string.Format("DrawRectangle出错:{0}", ex.Message), "ShowError");
                return false;
            }
        }
        public bool SaveRoi(int nCamID)
        {
            if (nCamID < 0)
                return false;

            return true;
        }
        public bool CreateShapeModel(int nCamID,EnumShapeModelType modelType,string modelName)
        {
            if (nCamID < 0)
                return false;
            switch (modelType)
            {
                case EnumShapeModelType.Gray:
                    break;
                case EnumShapeModelType.Shape:
                    break;
                case EnumShapeModelType.XLD:
                    string ModelFileName = $"VisionData\\Model\\Cam{nCamID}_{modelName}.shm";
                    string ModelOriginPosFileName = $"VisionData\\Model\\Cam{nCamID}_{modelName}.tup";
                    LdsFuncSet.CreateShapeModelXLD(ImageTemp, ModelFileName, ModelOriginPosFileName);
                    break;
                default:
                    return false;
            }
            return true;
        }
        public bool SaveImage(int nCamID,EnumImageType type, string filePath, string fileName,HTuple hWindow)
        {
            if (nCamID < 0)
                return false;
            if (!Directory.Exists(filePath))
                return false;
            switch (type)
            {
                case EnumImageType.Image:
                    HOperatorSet.WriteImage(ImageTemp, "jpeg", 0, $"{filePath}\\{fileName}");
                    break;
                case EnumImageType.Window:
                    HOperatorSet.DumpWindow(hWindow, "jpeg", $"{filePath}\\{fileName}");
                    break;
            }
            return true;
        }
        public Dictionary<string, Tuple<string, string>> FindCamera(EnumCamType camType)
        {
            Dictionary<string, Tuple<string, string>> dic = new Dictionary<string, Tuple<string, string>>();
            try
            {
                HOperatorSet.InfoFramegrabber(camType.ToString(), "info_boards", out HTuple hv_Information, out HTuple hv_ValueList);
                if (0 == hv_ValueList.Length)
                    return dic;
                for (int i = 0; i < Config.ConfigMgr.CameraCfgs.Length; i++)
                {
                    bool bFind = false;
                    foreach (var dev in hv_ValueList.SArr)
                    {
                        
                        string Name = dev.Substring(0, dev.IndexOf("port")).Replace("device:","").Trim();
                        if (Name.Contains(Config.ConfigMgr.CameraCfgs[i].Name))
                        {
                            dic.Add(Config.ConfigMgr.CameraCfgs[i].Name, new Tuple<string, string>(Name.Trim(), camType.ToString()));
                            bFind = true;
                            break;
                        }
                    }
                     if(!bFind)
                         Messenger.Default.Send<String>(string.Format("相机:{0}未找到硬件，请检查硬件连接或者配置", Config.ConfigMgr.CameraCfgs[i].Name), "ShowError");
                }
                return dic;
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<String>(string.Format("FIndCamera error:{0}", ex.Message));
                return dic;
                //throw new Exception(string.Format("FIndCamera error:{0}", ex.Message));
            }
        }
#endregion
    }

    public class VisionDataHelper
    {
        public static List<string> GetRoiListForSpecCamera(int nCamID,List<string> fileListInDataDirection)
        {
            var list = new List<string>();
            foreach (var it in fileListInDataDirection)
            {
                if (it.Contains(string.Format("Cam{0}_", nCamID)))
                    list.Add(it);
            }
            return list;
        }
        public static List<string> GetTemplateListForSpecCamera(int nCamID, List<string> fileListInDataDirection)
        {
            var list = new List<string>();
            foreach (var it in fileListInDataDirection)
            {
                if (it.Contains(string.Format("Cam{0}_", nCamID)))
                    list.Add(it);
            }
            return list;
        }

    }
}
