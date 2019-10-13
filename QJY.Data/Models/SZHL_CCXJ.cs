using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_CCXJ
    {
        public string LeiBie { get; set; }
        public DateTime? StarTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? Daycount { get; set; }
        public string ZhuYaoShiYou { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string Files { get; set; }
        public string Remark { get; set; }
        public int ID { get; set; }
        public int intProcessStanceid { get; set; }
        public int? ComId { get; set; }
    }
}
