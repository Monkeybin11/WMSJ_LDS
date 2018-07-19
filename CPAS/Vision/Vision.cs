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
    public enum Enum_REGION_OPERATOR { ADD, SUB }
    public enum Enum_REGION_TYPE { RECTANGLE, CIRCLE }
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
                Messenger.Default.Send<string>($"Open Camera Error:{CamCfgDic.ElementAt(nCamID)}:{ex.Message}", "ShowError");
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
        
        public bool SaveImage(int nCamID, EnumImageType type, string filePath, string fileName, HTuple hWindow)
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

                        string Name = dev.Substring(0, dev.IndexOf("port")).Replace("device:", "").Trim();
                        if (Name.Contains(Config.ConfigMgr.CameraCfgs[i].Name))
                        {
                            dic.Add(Config.ConfigMgr.CameraCfgs[i].Name, new Tuple<string, string>(Name.Trim(), camType.ToString()));
                            bFind = true;
                            break;
                        }
                    }
                    if (!bFind)
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

        #region LDS专用
        public bool GetAngleTune1(HObject image, HTuple hwindow,out double fAngle)
        {
            fAngle = 0;
            // Local iconic variables
            HObject ho_ImageScaled = null, ho_ImageMean = null;
            // Local control variables 
            HTuple hv_Index1, hv_EdgeGrayValue = new HTuple();
            HTuple hv_nSegment = new HTuple(), hv_Width = new HTuple();
            HTuple hv_Height = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple(), hv_Phi = new HTuple();
            HTuple hv_Length1 = new HTuple(), hv_Length2 = new HTuple();
            HTuple hv_RectanglePara = new HTuple(), hv_RecPara2 = new HTuple();
            HTuple hv_ModelPos = new HTuple(), hv_ModelID = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple(), hv_Angle = new HTuple();
            HTuple hv_Score = new HTuple(), hv_HomMat2D1 = new HTuple();
            HTuple hv_QRow = new HTuple(), hv_QColumn = new HTuple(), hv_Row2 = new HTuple();
            HTuple hv_Column2 = new HTuple(), hv_Phi2 = new HTuple(), hv_QRow2 = new HTuple();
            HTuple hv_QColumn2 = new HTuple(), hv_OutRowStart = new HTuple();
            HTuple hv_OutColStart = new HTuple(), hv_OutRowEnd = new HTuple();
            HTuple hv_OutColEnd = new HTuple(), hv_OutRowStart1 = new HTuple();
            HTuple hv_OutColStart1 = new HTuple(), hv_OutRowEnd1 = new HTuple();
            HTuple hv_OutColEnd1 = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);

            for (hv_Index1 = 1; (int)hv_Index1 <= 8; hv_Index1 = (int)hv_Index1 + 1)
            {
                hv_EdgeGrayValue = 8;
                hv_nSegment = 20;
                ho_ImageScaled.Dispose();
                scale_image_range(image, out ho_ImageScaled, 100,
                    200);
                HOperatorSet.GetImageSize(image, out hv_Width, out hv_Height);
                ho_ImageMean.Dispose();
                HOperatorSet.MeanImage(image, out ho_ImageMean, 50, 50);
                HOperatorSet.GetImageSize(ho_ImageMean, out hv_Width, out hv_Height);
                HOperatorSet.ReadTuple("RectanglePara.tup", out hv_RectanglePara);
                HOperatorSet.ReadTuple("RecPara2.tup", out hv_RecPara2);
                HOperatorSet.SetDraw(hwindow, "margin");
                HOperatorSet.SetColor(hwindow, "red");
                
                hv_Row = hv_RectanglePara[0];
                hv_Column = hv_RectanglePara[1];
                hv_Phi = hv_RectanglePara[2];
                hv_Length1 = hv_RectanglePara[3];
                hv_Length2 = hv_RectanglePara[4];
         
                HOperatorSet.ReadTuple("ModelPos.tup", out hv_ModelPos);
                HOperatorSet.ReadShapeModel("LdsShapeModel.shm", out hv_ModelID);
                HOperatorSet.FindShapeModel(image, hv_ModelID, (new HTuple(0)).TupleRad()
                    , (new HTuple(360)).TupleRad(), 0.7, 1, 0.5, "least_squares", 0, 0.9, out hv_Row1,
                    out hv_Column1, out hv_Angle, out hv_Score);

                //模板偏移
                HOperatorSet.VectorAngleToRigid(hv_ModelPos.TupleSelect(0), hv_ModelPos.TupleSelect(1), 0, hv_Row1, hv_Column1, hv_Angle, out hv_HomMat2D1);
                HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, hv_Row, hv_Column, out hv_QRow,out hv_QColumn);
                HOperatorSet.SetColor(hwindow, "green");
          
                hv_Row = hv_QRow.Clone();
                hv_Column = hv_QColumn.Clone();
                hv_Phi = hv_Phi + hv_Angle;


                //*
                hv_Row2 = hv_RecPara2[0];
                hv_Column2 = hv_RecPara2[1];
                hv_Phi2 = (hv_RecPara2.TupleSelect(2)) + hv_Angle;
                HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, hv_Row2, hv_Column2, out hv_QRow2, out hv_QColumn2);
                hv_Phi2 = (hv_RecPara2.TupleSelect(2)) + hv_Angle;
                hv_Row2 = hv_QRow2.Clone();
                hv_Column2 = hv_QColumn2.Clone();

                FindLine(ho_ImageScaled, hv_nSegment, hv_EdgeGrayValue, hv_Row, hv_Column, hv_Phi, hv_Length1, hv_Length2, out hv_OutRowStart, out hv_OutColStart, out hv_OutRowEnd, out hv_OutColEnd);
                FindLine(ho_ImageScaled, hv_nSegment, hv_EdgeGrayValue, hv_Row2, hv_Column2, hv_Phi2, hv_RecPara2.TupleSelect(3), hv_RecPara2.TupleSelect(4), out hv_OutRowStart1, out hv_OutColStart1, out hv_OutRowEnd1, out hv_OutColEnd1);
                HOperatorSet.SetColor(hwindow, "red");
                HOperatorSet.SetLineWidth(hwindow, 3);
                
                HOperatorSet.DispLine(hwindow, hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd);
                HOperatorSet.DispLine(hwindow, hv_OutRowStart1, hv_OutColStart1, hv_OutRowEnd1, hv_OutColEnd1);
            }
            image.Dispose();
            ho_ImageScaled.Dispose();
            ho_ImageMean.Dispose();
            return true;
        }
        #endregion

        public bool CreateShapeModel(int nCamID, EnumShapeModelType modelType, string modelName)
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
        private void FindLine(HObject ho_Image, HTuple hv_CaliperNum, HTuple hv_EdgeGrayValue, HTuple hv_RoiRow, HTuple hv_RoiCol, HTuple hv_RoiPhi, HTuple hv_RoiL1, HTuple hv_RoiL2, out HTuple hv_OutRowStart, out HTuple hv_OutColStart, out HTuple hv_OutRowEnd, out HTuple hv_OutColEnd)
        {
            // Local iconic variables 
            HObject ho_Rectangle, ho_Contour = null;
            // Local control variables 
            HTuple hv_Width, hv_Height, hv_newL2, hv_newL1;
            HTuple hv_Sin, hv_Cos, hv_BaseRow, hv_BaseCol, hv_newRow;
            HTuple hv_newCol, hv_ptRow, hv_ptCol, hv_nCount, hv_Index;
            HTuple hv_MeasureHandle = new HTuple(), hv_RowEdge = new HTuple();
            HTuple hv_ColumnEdge = new HTuple(), hv_Amplitude = new HTuple();
            HTuple hv_Distance = new HTuple(), hv_RowBegin = new HTuple();
            HTuple hv_ColBegin = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple(), hv_Nr = new HTuple(), hv_Nc = new HTuple();
            HTuple hv_Dist = new HTuple();

            HTuple hv_CaliperNum_COPY_INP_TMP = hv_CaliperNum.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Contour);

            hv_OutRowStart = new HTuple();
            hv_OutColStart = new HTuple();
            hv_OutRowEnd = new HTuple();
            hv_OutColEnd = new HTuple();
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle2(out ho_Rectangle, hv_RoiRow, hv_RoiCol, hv_RoiPhi,
                hv_RoiL1, hv_RoiL2);
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            //卡尺数量
            if ((int)(new HTuple(hv_CaliperNum_COPY_INP_TMP.TupleLessEqual(1))) != 0)
            {
                hv_CaliperNum_COPY_INP_TMP = 2;
            }
            hv_newL2 = hv_RoiL2 / (hv_CaliperNum_COPY_INP_TMP - 1);
            hv_newL1 = hv_RoiL1.Clone();
            HOperatorSet.TupleSin(hv_RoiPhi, out hv_Sin);
            HOperatorSet.TupleCos(hv_RoiPhi, out hv_Cos);

            hv_BaseRow = hv_RoiRow + (hv_RoiL2 * hv_Cos);
            hv_BaseCol = hv_RoiCol + (hv_RoiL2 * hv_Sin);

            hv_newRow = hv_BaseRow.Clone();
            hv_newCol = hv_BaseCol.Clone();
            hv_ptRow = new HTuple();
            hv_ptCol = new HTuple();
            hv_nCount = 0;
            for (hv_Index = 1; hv_Index.Continue(hv_CaliperNum_COPY_INP_TMP, 1); hv_Index = hv_Index.TupleAdd(1))
            {
                HOperatorSet.GenMeasureRectangle2(hv_newRow, hv_newCol, hv_RoiPhi, hv_newL1,
                    hv_newL2, hv_Width, hv_Height, "nearest_neighbor", out hv_MeasureHandle);
                HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, 1, hv_EdgeGrayValue, "negative",
                    "first", out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude, out hv_Distance);
                hv_newRow = hv_BaseRow - (((hv_newL2 * hv_Cos) * hv_Index) * 2);
                hv_newCol = hv_BaseCol - (((hv_newL2 * hv_Sin) * hv_Index) * 2);
                if ((int)(new HTuple((new HTuple(hv_RowEdge.TupleLength())).TupleGreater(0))) != 0)
                {
                    hv_ptRow[hv_nCount] = hv_RowEdge;
                    hv_ptCol[hv_nCount] = hv_ColumnEdge;
                    hv_nCount = hv_nCount + 1;
                }
                HOperatorSet.CloseMeasure(hv_MeasureHandle);
            }
            if ((int)(new HTuple((new HTuple(hv_ptRow.TupleLength())).TupleGreater(1))) != 0)
            {
                ho_Contour.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_ptRow, hv_ptCol);
                HOperatorSet.FitLineContourXld(ho_Contour, "tukey", -1, 0, 5, 2, out hv_RowBegin,
                    out hv_ColBegin, out hv_RowEnd, out hv_ColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
                hv_OutRowStart = hv_RowBegin.Clone();
                hv_OutColStart = hv_ColBegin.Clone();
                hv_OutRowEnd = hv_RowEnd.Clone();
                hv_OutColEnd = hv_ColEnd.Clone();
            }
            else
            {
                hv_OutRowStart = 0;
                hv_OutColStart = 0;
                hv_OutRowEnd = 0;
                hv_OutColEnd = 0;
            }
            ho_Rectangle.Dispose();
            ho_Contour.Dispose();

            return;
        }
        private void scale_image_range(HObject ho_Image, out HObject ho_ImageScaled, HTuple hv_Min, HTuple hv_Max)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];
            long SP_O = 0;

            // Local iconic variables 

            HObject ho_SelectedChannel = null, ho_LowerRegion = null;
            HObject ho_UpperRegion = null;

            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);


            // Local control variables 

            HTuple hv_LowerLimit = new HTuple(), hv_UpperLimit = new HTuple();
            HTuple hv_Mult, hv_Add, hv_Channels, hv_Index, hv_MinGray = new HTuple();
            HTuple hv_MaxGray = new HTuple(), hv_Range = new HTuple();

            HTuple hv_Max_COPY_INP_TMP = hv_Max.Clone();
            HTuple hv_Min_COPY_INP_TMP = hv_Min.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_SelectedChannel);
            HOperatorSet.GenEmptyObj(out ho_LowerRegion);
            HOperatorSet.GenEmptyObj(out ho_UpperRegion);

            //Convenience procedure to scale the gray values of the
            //input image Image from the interval [Min,Max]
            //to the interval [0,255] (default).
            //Gray values < 0 or > 255 (after scaling) are clipped.
            //
            //If the image shall be scaled to an interval different from [0,255],
            //this can be achieved by passing tuples with 2 values [From, To]
            //as Min and Max.
            //Example:
            //scale_image_range(Image:ImageScaled:[100,50],[200,250])
            //maps the gray values of Image from the interval [100,200] to [50,250].
            //All other gray values will be clipped.
            //
            //input parameters:
            //Image: the input image
            //Min: the minimum gray value which will be mapped to 0
            //     If a tuple with two values is given, the first value will
            //     be mapped to the second value.
            //Max: The maximum gray value which will be mapped to 255
            //     If a tuple with two values is given, the first value will
            //     be mapped to the second value.
            //
            //output parameter:
            //ImageScale: the resulting scaled image
            //
            if ((int)(new HTuple((new HTuple(hv_Min_COPY_INP_TMP.TupleLength())).TupleEqual(
                2))) != 0)
            {
                hv_LowerLimit = hv_Min_COPY_INP_TMP[1];
                hv_Min_COPY_INP_TMP = hv_Min_COPY_INP_TMP[0];
            }
            else
            {
                hv_LowerLimit = 0.0;
            }
            if ((int)(new HTuple((new HTuple(hv_Max_COPY_INP_TMP.TupleLength())).TupleEqual(
                2))) != 0)
            {
                hv_UpperLimit = hv_Max_COPY_INP_TMP[1];
                hv_Max_COPY_INP_TMP = hv_Max_COPY_INP_TMP[0];
            }
            else
            {
                hv_UpperLimit = 255.0;
            }
            //
            //Calculate scaling parameters
            hv_Mult = (((hv_UpperLimit - hv_LowerLimit)).TupleReal()) / (hv_Max_COPY_INP_TMP - hv_Min_COPY_INP_TMP);
            hv_Add = ((-hv_Mult) * hv_Min_COPY_INP_TMP) + hv_LowerLimit;
            //
            //Scale image
            OTemp[SP_O] = ho_Image_COPY_INP_TMP.CopyObj(1, -1);
            SP_O++;
            ho_Image_COPY_INP_TMP.Dispose();
            HOperatorSet.ScaleImage(OTemp[SP_O - 1], out ho_Image_COPY_INP_TMP, hv_Mult, hv_Add);
            OTemp[SP_O - 1].Dispose();
            SP_O = 0;
            //
            //Clip gray values if necessary
            //This must be done for each channel separately
            HOperatorSet.CountChannels(ho_Image_COPY_INP_TMP, out hv_Channels);
            for (hv_Index = 1; hv_Index.Continue(hv_Channels, 1); hv_Index = hv_Index.TupleAdd(1))
            {
                ho_SelectedChannel.Dispose();
                HOperatorSet.AccessChannel(ho_Image_COPY_INP_TMP, out ho_SelectedChannel, hv_Index);
                HOperatorSet.MinMaxGray(ho_SelectedChannel, ho_SelectedChannel, 0, out hv_MinGray,
                    out hv_MaxGray, out hv_Range);
                ho_LowerRegion.Dispose();
                HOperatorSet.Threshold(ho_SelectedChannel, out ho_LowerRegion, ((hv_MinGray.TupleConcat(
                    hv_LowerLimit))).TupleMin(), hv_LowerLimit);
                ho_UpperRegion.Dispose();
                HOperatorSet.Threshold(ho_SelectedChannel, out ho_UpperRegion, hv_UpperLimit,
                    ((hv_UpperLimit.TupleConcat(hv_MaxGray))).TupleMax());
                OTemp[SP_O] = ho_SelectedChannel.CopyObj(1, -1);
                SP_O++;
                ho_SelectedChannel.Dispose();
                HOperatorSet.PaintRegion(ho_LowerRegion, OTemp[SP_O - 1], out ho_SelectedChannel,
                    hv_LowerLimit, "fill");
                OTemp[SP_O - 1].Dispose();
                SP_O = 0;
                OTemp[SP_O] = ho_SelectedChannel.CopyObj(1, -1);
                SP_O++;
                ho_SelectedChannel.Dispose();
                HOperatorSet.PaintRegion(ho_UpperRegion, OTemp[SP_O - 1], out ho_SelectedChannel,
                    hv_UpperLimit, "fill");
                OTemp[SP_O - 1].Dispose();
                SP_O = 0;
                if ((int)(new HTuple(hv_Index.TupleEqual(1))) != 0)
                {
                    ho_ImageScaled.Dispose();
                    HOperatorSet.CopyObj(ho_SelectedChannel, out ho_ImageScaled, 1, 1);
                }
                else
                {
                    OTemp[SP_O] = ho_ImageScaled.CopyObj(1, -1);
                    SP_O++;
                    ho_ImageScaled.Dispose();
                    HOperatorSet.AppendChannel(OTemp[SP_O - 1], ho_SelectedChannel, out ho_ImageScaled
                        );
                    OTemp[SP_O - 1].Dispose();
                    SP_O = 0;
                }
            }
            ho_Image_COPY_INP_TMP.Dispose();
            ho_SelectedChannel.Dispose();
            ho_LowerRegion.Dispose();
            ho_UpperRegion.Dispose();

            return;
        }
    }

    public class VisionDataHelper
    {
        public static List<string> GetRoiListForSpecCamera(int nCamID, List<string> fileListInDataDirection)
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
