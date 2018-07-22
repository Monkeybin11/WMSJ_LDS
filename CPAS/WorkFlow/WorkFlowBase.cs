using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPAS.Config;
using CPAS.Config.SoftwareManager;
using CPAS.Models;
using CPSA.CLasses;
using GalaSoft.MvvmLight.Messaging;

namespace CPAS.WorkFlow
{
    public class WorkFlowBase
    {
        protected Dictionary<string, LDSModel> LdsDic = new Dictionary<string, LDSModel>();//扫描不到直接NG
        public bool Enable;
        public string StationName;
        public int StationIndex;
        protected WorkFlowConfig cfg = null;
        protected CancellationTokenSource cts =new CancellationTokenSource();
        protected Stack<object> nStepStack=new Stack<object>();
        protected Task t = null; 
        protected object nStep;
        protected object PeekStep() { return nStepStack.Peek(); }
        protected void PushStep(object nStep) { nStepStack.Push(nStep); }
        protected void PopAndPushStep(object nStep) { nStepStack.Pop(); nStepStack.Push(nStep); }
        protected void PushBatchStep(object[] nSteps)
        {
            foreach (var step in nSteps)
                nStepStack.Push(step);
        }
        protected void PopStep() { nStepStack.Pop(); }
        protected void ClearAllStep() { nStepStack.Clear(); }
        protected int GetCurStepCount() { return nStepStack.Count; }
        protected virtual bool UserInit() { return true; }
        public WorkFlowBase(WorkFlowConfig cfg) { this.cfg = cfg;  t = new Task(() => ThreadFunc(this), cts.Token); }
        public void ShowInfo(string strInfo=null)    //int msg, int iPara, object lParam
        {
            if (strInfo == null || strInfo.Trim().ToString() == "")
                strInfo = nStep.ToString();
            DateTime dt = DateTime.Now;
            //Messenger.Default.Send<Tuple<string,string>>(new Tuple<string, string>(cfg.Name, string.Format("{0:D2}:{1:D2}:{2:D2}  {3:D2}", dt.Hour, dt.Minute, dt.Second, strInfo)), "ShowStepInfo");
        }
        public bool Start()
        {
            if (!UserInit())
            {
                LogHelper.WriteLine($"工站{cfg.Name}初始化UserInit失败,无法启动流程");
                return false;
            }
            if (t.Status == TaskStatus.Created)
                t.Start();
            else if (t.Status == TaskStatus.Canceled || t.Status == TaskStatus.RanToCompletion)
            {
                cts = new CancellationTokenSource();
                t = new Task(() => ThreadFunc(this), cts.Token);
                t.Start();
            }
            return true;
        }
        public bool Stop()
        {
            cts.Cancel();
            return true;
        }
        private static int ThreadFunc(object o) { return (o as WorkFlowBase).WorkFlow(); }
        protected virtual int WorkFlow() { return 0; }
        public void WaitComplete()
        {
            //if (t != null)
            //    t.Wait(5000);
        }

        protected PrescriptionGridModel Prescription = null;   //配方信息
        protected SystemParaModel SysPara = null;
        protected bool GetPresInfomation()
        {
            SysPara = ConfigMgr.SystemParaCfgMgr.SystemParaModels[0];
            if (SysPara != null)
            {
                var pres  = from it in ConfigMgr.PrescriptionCfgMgr.Prescriptions where it.Name == SysPara.CurPrescriptionName select it;
                if (pres.Count() > 0)
                    Prescription = pres.First();
            }
            return SysPara != null && Prescription != null;
        }
    }
}
