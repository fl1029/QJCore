using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class BI_DB_Source
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DBType { get; set; }
        public string DBIP { get; set; }
        public string Port { get; set; }
        public string DBName { get; set; }
        public string Schema { get; set; }
        public string DBUser { get; set; }
        public string DBPwd { get; set; }
        public string Attach { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
