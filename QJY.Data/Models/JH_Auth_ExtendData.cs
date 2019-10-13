using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_ExtendData
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string TableName { get; set; }
        public int? ExtendModeID { get; set; }
        public int? DataID { get; set; }
        public string ExtendDataValue { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public int? BranchNo { get; set; }
        public int? PDID { get; set; }
        public string ExFiledName { get; set; }
        public string ExFiledColumn { get; set; }
        public string CRUserName { get; set; }
        public string BranchName { get; set; }
    }
}
