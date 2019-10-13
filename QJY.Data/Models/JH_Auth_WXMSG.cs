using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_WXMSG
    {
        public int ID { get; set; }
        public int? ComId { get; set; }
        public string MsgType { get; set; }
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string MediaId { get; set; }
        public string ThumbMediaId { get; set; }
        public string MsgId { get; set; }
        public int? AgentID { get; set; }
        public string MsgContent { get; set; }
        public string PicUrl { get; set; }
        public string URL { get; set; }
        public string Format { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string FileId { get; set; }
        public string ModeCode { get; set; }
        public string Tags { get; set; }
    }
}
