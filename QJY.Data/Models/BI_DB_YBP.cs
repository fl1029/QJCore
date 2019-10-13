using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class BI_DB_YBP
    {
        public int ID { get; set; }
        public int? DimID { get; set; }
        public string Name { get; set; }
        public string YBContent { get; set; }
        public string YBType { get; set; }
        public string YBOption { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string Remark { get; set; }
    }
}
