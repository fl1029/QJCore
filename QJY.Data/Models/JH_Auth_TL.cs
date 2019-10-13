using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_TL
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string MSGType { get; set; }
        public string MSGTLYID { get; set; }
        public string MSGTLHFID { get; set; }
        public string MSGContent { get; set; }
        public string MSGisHasFiles { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string CRUserName { get; set; }
        public string MSGISDele { get; set; }
        public string MsgISShow { get; set; }
        public string Remark { get; set; }
        public string Remark1 { get; set; }
        public int? Points { get; set; }
        public int? TLID { get; set; }
        public string ReUser { get; set; }
    }
}
