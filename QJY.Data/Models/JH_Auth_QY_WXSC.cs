using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_QY_WXSC
    {
        public int ComId { get; set; }
        public int ID { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string SCTemp { get; set; }
        public string SCType { get; set; }
        public string SCPicUrl { get; set; }
        public string SCFileUrl { get; set; }
    }
}
