using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_QY_Model
    {
        public int ID { get; set; }
        public int? ComId { get; set; }
        public int? ModelID { get; set; }
        public string Status { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public int? PDID { get; set; }
        public string AgentId { get; set; }
        public string QYModelCode { get; set; }
        public string ModelAdmin { get; set; }
        public int? IsInit { get; set; }
    }
}
