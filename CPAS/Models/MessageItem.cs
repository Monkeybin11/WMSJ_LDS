using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPAS.Models
{
    public enum MSGTYPE
    {
        INFO,
        WARNING,
        ERROR,
    }
    public class MessageItem
    {
        public MessageItem()
        {
            StrTime = DateTime.Now.GetDateTimeFormats()[35];
        }
        public MSGTYPE MsgType { get; set; }
        public string StrMsg { get; set; }
        public string StrTime { get; set; }
    }
}
