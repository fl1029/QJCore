using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_XTDT
    {
        public string ComId { get; set; }
        public int ID { get; set; }
        public string DTContent { get; set; }
        public string Remark { get; set; }
        public int? DataId { get; set; }
        public string DTType { get; set; }
        public string DTLink { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
