using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_ExtendMode
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string TableName { get; set; }
        public string TableFiledColumn { get; set; }
        public string TableFiledName { get; set; }
        public string TableFileType { get; set; }
        public string DefaultOption { get; set; }
        public string DefaultValue { get; set; }
        public string IsRequire { get; set; }
        public int? PDID { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
