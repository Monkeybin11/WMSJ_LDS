using Microsoft.VisualStudio.TestTools.UnitTesting;
using CPAS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPAS.Instrument;
using CPAS.Config.HardwareManager;
using CPAS.Classes;
using CPAS.Models;
namespace CPAS.Views.Tests
{
    
    [TestClass()]
    public class LDsTest
    {
        #region Test LDS
        private HardwareCfgLevelManager1 hdCfg = new HardwareCfgLevelManager1()
        {
            ConnectMode = "COMPORT",
            InstrumentName = "LDS[0]",
            Enabled = true,
            PortName = "LDSPort"
        };
        private ComportCfg cfg = new ComportCfg()
        {
            BaudRate = 115200,
            Parity = "none",
            DataBits = 8,
            Port = "COM9",
            PortName = "LDSPort",
            StopBits = 1,
            TimeOut = 1000
        };
        private LDS lds = null;
        public LDsTest()
        {
            lds = new LDS(hdCfg);
        }
        [TestMethod()]
        public void Init()
        {
            bool bRet = lds.MyInit(cfg);
            Assert.IsTrue(bRet);
        }
        [TestMethod()]
        public void WriteSerialNumber()
        {
            bool bRet = lds.MyInit(cfg);
            Assert.IsTrue(bRet);
            string strID = new string('5', 21);
            bRet = lds.DoRecord(strID);
            Assert.IsTrue(bRet);
        }
        [TestMethod()]
        public void GetExposeValue()
        {
            bool bRet = lds.MyInit(cfg);
            Assert.IsTrue(bRet);

            int Value = lds.GetExposeValue(1536);
            Assert.IsTrue(Value > 0);

        }
        [TestMethod()]
        public void TestUnlock()
        {
            bool bRet = lds.MyInit(cfg);
            bRet=lds.LdsUnLock(out string strError);
            Console.WriteLine(strError);
            Assert.IsTrue(bRet);
        }
        #endregion

        #region TestPM800
        [TestMethod()]
        public void TestReadPower()
        {
            PowerMeter pm = new PowerMeter(null);
            bool bRet = pm.Init();
            Assert.IsTrue(bRet);
            double Value = pm.GetPowerValue(EnumUnit.μW);
            Console.WriteLine(Value);
        }

        #endregion

        #region TestKeyence2000
        [TestMethod()]
        public void TestReadBarcode()
        {
            Keyence_SR1000 keyence = new Keyence_SR1000(null);
            bool bRet = keyence.MyInit();
            Assert.IsTrue(bRet);
            string strCode = keyence.Getbarcode();
            Console.WriteLine(strCode);
        }
        #endregion

        #region TestLDSModelResult
        [TestMethod()]
        public void TestPushInOut()
        {
            LDSResultModelMgr.Instance.PushLdsIn(new LDSResultModel() { SN="111",AdjustFocusOK = true, AdjustHorizOK = true });
            LDSResultModelMgr.Instance.PushLdsIn(new LDSResultModel() { SN = "222", AdjustFocusOK = true, AdjustHorizOK = true });
            LDSResultModelMgr.Instance.PushLdsIn(new LDSResultModel() { SN = "333", AdjustFocusOK = true, AdjustHorizOK = true });
            LDSResultModelMgr.Instance.PushLdsIn(new LDSResultModel() { SN = "44", AdjustFocusOK = true, AdjustHorizOK = true });
            var it = LDSResultModelMgr.Instance.PopOut();
            it = LDSResultModelMgr.Instance.PopOut();
            it = LDSResultModelMgr.Instance.PopOut();
            it = LDSResultModelMgr.Instance.PopOut();

            it = LDSResultModelMgr.Instance.PopOut();
            it = LDSResultModelMgr.Instance.PopOut();
            it = LDSResultModelMgr.Instance.PopOut();
        }
        #endregion
    }
}