using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Log
    {
        public string ComId { get; set; }
        public int ID { get; set; }
        public string LogType { get; set; }
        public string LogContent { get; set; }
        public string Remark { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string IP { get; set; }
        public string Remark1 { get; set; }
        public string netcode { get; set; }
    }
}
