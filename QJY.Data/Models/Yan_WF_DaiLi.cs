using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class Yan_WF_DaiLi
    {
        public string ComId { get; set; }
        public int ID { get; set; }
        public string DaiLiRen { get; set; }
        public string BDaiLiRen { get; set; }
        public DateTime? DLSDate { get; set; }
        public DateTime? DLEDate { get; set; }
        public string Remark3 { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
