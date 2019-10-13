using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_YYLog
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string QYName { get; set; }
        public string CorpID { get; set; }
        public string TJID { get; set; }
        public string ModelCode { get; set; }
        public string UserName { get; set; }
        public string UserRealName { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string Type { get; set; }
        public string Event { get; set; }
        public string EventKey { get; set; }
        public string AgentID { get; set; }
        public int? ModelID { get; set; }
        public string Remark { get; set; }
    }
}
