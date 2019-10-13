using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_UserCustomData
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string UserName { get; set; }
        public string DataType { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string Remark { get; set; }
        public string DataContent { get; set; }
        public string DataContent1 { get; set; }
        public int? WXBQCode { get; set; }
    }
}
