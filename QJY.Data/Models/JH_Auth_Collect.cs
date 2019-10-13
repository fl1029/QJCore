using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Collect
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public int? RefID { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string CollectType { get; set; }
        public string CollectContent { get; set; }
        public string Remark1 { get; set; }
    }
}
