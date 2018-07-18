using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
namespace LDSFuncSet
{
    public class LDSVisionFunc
    {
        public bool CreateShapeModelXLD(HObject ImageIn,HTuple ModelName,HTuple ModelOriginPos)
        {
            // Local iconic variables 

            HObject  ho_ImageMean;
            HObject ho_Regions, ho_RegionFillUp, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_RegionTrans, ho_RegionDilation;
            HObject ho_RegionErosion, ho_RegionDifference, ho_ImageReduced;
            HObject ho_Regions2, ho_RegionFillUp1, ho_Contours1;


            // Local control variables 
            HTuple hv_Width, hv_Height;
            HTuple hv_ModelID;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Regions2);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
            HOperatorSet.GenEmptyObj(out ho_Contours1);

           
            HOperatorSet.GetImageSize(ImageIn, out hv_Width, out hv_Height);
            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ImageIn, out ho_ImageMean, 50, 50);
 
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_ImageMean, out ho_Regions, 143, 255);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Regions, out ho_RegionFillUp);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionFillUp, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area", 0);
            ho_RegionTrans.Dispose();
            HOperatorSet.ShapeTrans(ho_SelectedRegions, out ho_RegionTrans, "outer_circle");
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationCircle(ho_RegionTrans, out ho_RegionDilation, 50);
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionTrans, out ho_RegionErosion, 150);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionDilation, ho_RegionErosion, out ho_RegionDifference);
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ImageIn, ho_RegionDifference, out ho_ImageReduced);
            ho_Regions2.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced, out ho_Regions2, 204, 255);
            ho_RegionFillUp1.Dispose();
            HOperatorSet.FillUpShape(ho_Regions2, out ho_RegionFillUp1, "area", 1, 1000);
            ho_Contours1.Dispose();
            HOperatorSet.GenContourRegionXld(ho_RegionFillUp1, out ho_Contours1, "border");
            HOperatorSet.CreateShapeModelXld(ho_Contours1, "auto", (new HTuple(0)).TupleRad()  , (new HTuple(360)).TupleRad(), "auto", "auto", "ignore_local_polarity",  5, out hv_ModelID);
            HOperatorSet.FindShapeModel(ImageIn, hv_ModelID, (new HTuple(0)).TupleRad() , (new HTuple(360)).TupleRad(), 0.5, 1, 0.5, "least_squares", 0, 0.9, out HTuple hv_Row3, out HTuple hv_Column3, out HTuple hv_Angle2, out HTuple hv_Score);
            
            //保存建立模板的时候Model的原始位置信息
            ModelOriginPos[0] = hv_Row3;
            ModelOriginPos[1] = hv_Column3;
            ModelOriginPos[2] = hv_Angle2;


            HOperatorSet.WriteShapeModel(hv_ModelID, ModelName);
            ImageIn.Dispose();
            ho_ImageMean.Dispose();
            ho_Regions.Dispose();
            ho_RegionFillUp.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_RegionTrans.Dispose();
            ho_RegionDilation.Dispose();
            ho_RegionErosion.Dispose();
            ho_RegionDifference.Dispose();
            ho_ImageReduced.Dispose();
            ho_Regions2.Dispose();
            ho_RegionFillUp1.Dispose();
            ho_Contours1.Dispose();
            return true;
        }
        public bool FindShapeModelXLD(HObject ImageIn, HTuple ModelName)
        {
            HObject ho_ModelContours, ho_ImageMean;
            HObject ho_Regions, ho_RegionFillUp, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_RegionTrans, ho_Contours;
            HObject ho_RegionDilation, ho_RegionErosion, ho_RegionDifference;
            HObject ho_ImageReduced, ho_ContoursAffinTrans;


            // Local control variables 

            HTuple hv_ModelID1, hv_Row3, hv_Column3, hv_Angle2;
            HTuple hv_Score, hv_HomMat2D;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ModelContours);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ContoursAffinTrans);

            HOperatorSet.ReadShapeModel("LdsShapeModel.shm", out hv_ModelID1);
            ho_ModelContours.Dispose();
            HOperatorSet.GetShapeModelContours(out ho_ModelContours, hv_ModelID1, 1);

            HOperatorSet.MeanImage(ImageIn, out ho_ImageMean, 50, 50);
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_ImageMean, out ho_Regions, 143, 255);

            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_Regions, out ho_RegionFillUp);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionFillUp, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area", 0);
            ho_RegionTrans.Dispose();
            HOperatorSet.ShapeTrans(ho_SelectedRegions, out ho_RegionTrans, "outer_circle");
            ho_Contours.Dispose();
            HOperatorSet.GenContourRegionXld(ho_RegionTrans, out ho_Contours, "border");

            ho_RegionDilation.Dispose();
            HOperatorSet.DilationCircle(ho_RegionTrans, out ho_RegionDilation, 50);
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionTrans, out ho_RegionErosion, 150);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionDilation, ho_RegionErosion, out ho_RegionDifference );
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ImageIn, ho_RegionDifference, out ho_ImageReduced);
            HOperatorSet.FindShapeModel(ho_ImageReduced, hv_ModelID1, (new HTuple(0)).TupleRad()
                , (new HTuple(360)).TupleRad(), 0.5, 1, 0.5, "least_squares", 0, 0.9, out hv_Row3,
                out hv_Column3, out hv_Angle2, out hv_Score);
            HOperatorSet.VectorAngleToRigid(0, 0, 0, hv_Row3, hv_Column3, hv_Angle2, out hv_HomMat2D);
            ho_ContoursAffinTrans.Dispose();
            HOperatorSet.AffineTransContourXld(ho_ModelContours, out ho_ContoursAffinTrans, hv_HomMat2D);
           
            ho_ModelContours.Dispose();
            ImageIn.Dispose();
            ho_ImageMean.Dispose();
            ho_Regions.Dispose();
            ho_RegionFillUp.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_RegionTrans.Dispose();
            ho_Contours.Dispose();
            ho_RegionDilation.Dispose();
            ho_RegionErosion.Dispose();
            ho_RegionDifference.Dispose();
            ho_ImageReduced.Dispose();
            ho_ContoursAffinTrans.Dispose();
            return true;
        }
        public bool SaveRoi()
        {
            return true;
        }
    }
}
