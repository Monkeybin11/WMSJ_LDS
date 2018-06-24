using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPAS.Classes
{
    public class C
    {

        #region PLC寄存器设置
        public static string wHeartBeat_PLC_PC = "R0";
        public static string wHeartBeat_PC_PLC = "R1";

        //UnLock
        public static string wUnLockLDS_1_Req_PLC_PC = "R14";        
        public static string wUnLockLDS_1_Result_PC_PLC = "R15";

        //Barcode
        public static string wGetbarcode_1_Req_PLC_PC = "R17";
        public static string wGetBarode_1_Result_PC_PLC = "R18";
        public static string wBarode_1_StartAdd_PC_PLC = "R19";

        public static string wGetbarcode_2_Req_PLC_PC = "R33";
        public static string wGetBarode_2_Result_PC_PLC = "R34";
        public static string wBarode_2_StartAdd_PC_PLC = "R35";



        //AdjustLaser
        public static string wAdjustLaser_1_Enable_PC_PLC = "R46";
        public static string wAdjustLaser_1_Req_PLC_PC = "R47";
        public static string wAdjustLaser_1_Result_PC_PLC = "R48";


        public static string wAdjustLaser_2_Req_PLC_PC = "R62";
        public static string wAdjustLaser_2_Result_PC_PLC = "R63";


        //AdjustHorization
        public static string wAdjustHorization_Enable_PC_PLC = "R87";
        public static string wAdjustHorization_1_Req_PLC_PC = "R88";
        public static string wAdjustHorization_1_BoolResult_PC_PLC = "R89";
        public static string dwAdjustHorization_1_Angle_PC_PLC = "R90";
        public static string wAdjustHorization_1_Barcode_Start_Add_PLC_PC = "R90";      //调整水平工位的二维码1

        public static string wAdjustHorization_2_Req_PLC_PC = "R107";
        public static string wAdjustHorization_2_BoolResult_PC_PLC = "R108";
        public static string dwAdjustHorization_2_Angle_PC_PLC = "R109";
        public static string wAdjustHorization_2_Barcode_Start_Add_PLC_PC = "R111";      //调整水平工位的二维码2


        #endregion


    }
}
