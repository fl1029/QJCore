using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_TXSX
    {
        public int ID { get; set; }
        public int? ComId { get; set; }
        public string Type { get; set; }
        public string TXUser { get; set; }
        public string TXContent { get; set; }
        public string Remark { get; set; }
        public string TXType { get; set; }
        public string Days { get; set; }
        public string Date { get; set; }
        public string Hour { get; set; }
        public string Minute { get; set; }
        public string CFType { get; set; }
        public int? CFCount { get; set; }
        public DateTime? CFJZDate { get; set; }
        public int? ZXCount { get; set; }
        public string PCLink { get; set; }
        public string WXLink { get; set; }
        public string Status { get; set; }
        public string CRUser { get; set; }
        public string CRUserRealName { get; set; }
        public DateTime? CRDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateRealName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int intProcessStanceid { get; set; }
        public DateTime? LstSendTime { get; set; }
        public string MsgID { get; set; }
        public string APIName { get; set; }
        public string FunName { get; set; }
        public string TXMode { get; set; }
        public string ISCS { get; set; }
    }
}
