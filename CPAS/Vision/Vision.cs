using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;

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
            T1,
            T2,
            T3,
            T4
        }
        private List<HObject> HoImageList = new List<HObject>(10);    //Image
        private List<HTuple> AcqHandleList = new List<HTuple>(10);    //Aqu
        private Dictionary<int, Dictionary<string, HTuple>> HwindowDic = new Dictionary<int, Dictionary<string, HTuple>>();    //Hwindow
        private Dictionary<int, Tuple<HTuple, HTuple>> ActiveCamDic = new Dictionary<int, Tuple<HTuple, HTuple>>();
        private HObject Region = null;
        public Enum_REGION_OPERATOR RegionOperator = Enum_REGION_OPERATOR.ADD;
        public Enum_REGION_TYPE RegionType = Enum_REGION_TYPE.CIRCLE;
        private HObject ImageTemp = null;
        #endregion

        #region public method 
        public bool AttachCamWIndow(int nCamID, string Name, HTuple hWindow)
        {
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
            HObject image = null;
            HTuple hv_AcqHandle = null;
            HTuple width = 0, height = 0;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!IsCamOpen(nCamID))
                    {
                        HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                                                    -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
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
                Messenger.Default.Send<string>(ex.Message, "ShowError");
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
            lock (_lockList[nCamID])
            {
                return ActiveCamDic.Keys.Contains(nCamID);
            }
        }
        public void GrabImage(int nCamID, bool bDispose = true)
        {
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
                    {
                        Messenger.Default.Send<string>(string.Format("打开相机{0}失败", nCamID), "ShowError");
                        return;
                    }
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
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    switch (nStep)
                    {
                        case IMAGEPROCESS_STEP.T1:

                            break;
                        case IMAGEPROCESS_STEP.T2:
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
