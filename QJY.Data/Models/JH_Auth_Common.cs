using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Common
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public string Url { get; set; }
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string ModelCode { get; set; }
        public string Remark { get; set; }
        public string Sort { get; set; }
        public int? TopID { get; set; }
        public string Status { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
