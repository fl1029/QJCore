using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_TXL
    {
        public int ID { get; set; }
        public string LXName { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string TagName { get; set; }
        public string UPUser { get; set; }
        public DateTime? UPDDate { get; set; }
        public string LXHM { get; set; }
        public string LXMail { get; set; }
        public string LXRemark { get; set; }
        public int? ComId { get; set; }
    }
}
