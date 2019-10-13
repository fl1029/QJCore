using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class BI_DB_Dim
    {
        public int ID { get; set; }
        public int? STID { get; set; }
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string Dimension { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string ColumnSource { get; set; }
    }
}
