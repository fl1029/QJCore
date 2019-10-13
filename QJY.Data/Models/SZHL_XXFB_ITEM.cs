using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_XXFB_ITEM
    {
        public int ID { get; set; }
        public string XXTitle { get; set; }
        public string XXContent { get; set; }
        public DateTime? FBTime { get; set; }
        public string remark { get; set; }
        public string remark1 { get; set; }
        public string remark2 { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string CRUserName { get; set; }
        public string ReadUser { get; set; }
        public int? MsgType { get; set; }
        public int? ComId { get; set; }
        public int? isPL { get; set; }
        public int? XXFBId { get; set; }
        public string Files { get; set; }
        public string Author { get; set; }
        public string ImageIds { get; set; }
    }
}
