using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_GZGL_FL
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal? Amount { get; set; }
        public int? Rules { get; set; }
        public string Files { get; set; }
        public string Remark { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
