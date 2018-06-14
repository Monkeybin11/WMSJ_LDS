using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;

namespace CPAS.Vision
{
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
        }
        private static readonly Lazy<Vision> _instance = new Lazy<Vision>(() => new Vision());
        public static Vision Instance
        {
            get { return _instance.Value; }
        }
        public List<object> _lockList = new List<object>();
        #endregion

        #region  var
        public enum IMAGEPROCESS_STEP
        {
            T1,
            T2,
            T3,
            T4           
        }
        private List<HObject> HoImageList=new List<HObject>(10) ;    //Image
        private List<HTuple> AcqHandleList=new List<HTuple>(10);    //Aqu
        private Dictionary<int, Dictionary<string,HTuple>> HwindowDic = new Dictionary<int, Dictionary<string, HTuple>>();    //Hwindow
        private Dictionary<int,Tuple<HTuple,HTuple>> ActiveCamDic = new Dictionary<int, Tuple<HTuple, HTuple>>();
        #endregion

        #region public method 
        public bool AttachCamWIndow(int nCamID,string Name, HTuple hWindow)
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
                        HwindowDic[nCamID][Name]=hWindow;
                }
                else
                    HwindowDic.Add(nCamID, new Dictionary<string, HTuple>() { { Name,hWindow} });
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
            HTuple width=0,height=0;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!ActiveCamDic.Keys.Contains(nCamID))
                    {
                        HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                                                    -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
                        HOperatorSet.GrabImage(out image, hv_AcqHandle);
                        HOperatorSet.GetImageSize(image, out width, out height);
                        ActiveCamDic.Add(nCamID, new Tuple<HTuple, HTuple>(width, height));
                    }
                    if (HwindowDic.Keys.Contains(nCamID))
                    {
                        foreach (var it in HwindowDic[nCamID])
                        {
                            HOperatorSet.SetPart(it.Value, 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);
                            HOperatorSet.DispObj(image, it.Value);
                        }
                    }
                    AcqHandleList[nCamID] = hv_AcqHandle;
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
                        HOperatorSet.CloseFramegrabber(AcqHandleList[nCamID]);
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
                return HwindowDic.Keys.Contains(nCamID);
            }
        }
        public void GrabImage(int nCamID,bool bDispose=true)
        {
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!HwindowDic.Keys.Contains(nCamID) || !ActiveCamDic.Keys.Contains(nCamID))
                    {
                        Messenger.Default.Send<string>("请先绑定视觉窗口或者相机没有打开", "ShowError");
                        return;
                    }
                    HOperatorSet.GrabImage(out image, AcqHandleList[nCamID]);
                    foreach (var it in HwindowDic[nCamID])
                        HOperatorSet.DispObj(image, it.Value);
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
            }
            finally
            {
                if(bDispose && image!=null)
                    image.Dispose();
            }
        }
        public bool ProcessImage(IMAGEPROCESS_STEP nStep,int nCamID,object para,out object result)
        {
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    HOperatorSet.GrabImage(out image, AcqHandleList[nCamID]);
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
        #endregion

        #region private method

        #endregion

    }
}
