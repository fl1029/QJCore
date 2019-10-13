using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class BI_DB_Set
    {
        public int ID { get; set; }
        public int? SID { get; set; }
        public string Name { get; set; }
        public string SName { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public DateTime? UPDate { get; set; }
        public string Type { get; set; }
        public string Option { get; set; }
        public string DSQL { get; set; }
    }
}
