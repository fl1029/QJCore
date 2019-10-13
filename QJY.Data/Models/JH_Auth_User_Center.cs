using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_User_Center
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string UserTO { get; set; }
        public string UserFrom { get; set; }
        public string MsgType { get; set; }
        public string MsgModeID { get; set; }
        public string MsgContent { get; set; }
        public int? IsSend { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string DataId { get; set; }
        public string MsgLink { get; set; }
        public string wxLink { get; set; }
        public string Remark { get; set; }
        public DateTime? ReadDate { get; set; }
        public int? isRead { get; set; }
        public string isCS { get; set; }
    }
}
