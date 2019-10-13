using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_YCGL
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public int? CarID { get; set; }
        public string CarName { get; set; }
        public string XCType { get; set; }
        public string JSR { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string StartAddress { get; set; }
        public string EndAddress { get; set; }
        public string Files { get; set; }
        public string SYUser { get; set; }
        public string SYDec { get; set; }
        public int? SYRS { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public int intProcessStanceid { get; set; }
        public int? IsDel { get; set; }
        public string DelUser { get; set; }
        public DateTime? DelDate { get; set; }
        public DateTime? BackDate { get; set; }
    }
}
