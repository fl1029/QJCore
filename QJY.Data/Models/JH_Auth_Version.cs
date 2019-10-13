using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Version
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string Version { get; set; }
        public string VersionContent { get; set; }
        public string ReadUsers { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string Remark { get; set; }
    }
}
