using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPAS.Models;
namespace CPAS.Classes
{
    enum EnumStation
    {
        T1, //解锁，扫码，调功率值
        T2,
        T3,
        T4
    }
    public class LDSResultModelMgr
    {
        private LDSResultModelMgr() { }
        private static readonly Lazy<LDSResultModelMgr> _instance = new Lazy<LDSResultModelMgr>(() => new LDSResultModelMgr());
        public static LDSResultModelMgr Instance
        {
            get { return _instance.Value; }
        }
        private List<LDSResultModel> LdsResultList= new List<LDSResultModel>{null,null,null,null,null,null,null,null };

        public void PushLdsIn(LDSResultModel model)
        {
            LdsResultList.Add(model);
        }
        public LDSResultModel PopOut()
        {
            var model= LdsResultList.Last();
            LdsResultList.RemoveAt(LdsResultList.Count - 1);    //移除最后一个
            return model;
        }

    }
}
