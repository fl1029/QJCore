using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_XXFB
    {
        public int ID { get; set; }
        public string XXTitle { get; set; }
        public int? isPL { get; set; }
        public int? intProcessStanceid { get; set; }
        public int? XXFBType { get; set; }
        public int? MsgType { get; set; }
        public string JSUser { get; set; }
        public int? ComId { get; set; }
        public string IsSecret { get; set; }
        public DateTime? FBTime { get; set; }
        public string Files { get; set; }
        public string BZDanWei { get; set; }
        public string SHUser { get; set; }
        public DateTime? SHDate { get; set; }
        public int? SHStatus { get; set; }
        public string SHYJ { get; set; }
        public string IsSend { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUserName { get; set; }
        public string Remark { get; set; }
        public string SCUser { get; set; }
        public string IsSH { get; set; }
    }
}
