using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Model
    {
        public int ID { get; set; }
        public string ModelName { get; set; }
        public string ModelType { get; set; }
        public string Remark1 { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string TJId { get; set; }
        public string ModelStatus { get; set; }
        public string ModePicUrl { get; set; }
        public string AppID { get; set; }
        public string ModelUrl { get; set; }
        public string IsSQ { get; set; }
        public string ModelCode { get; set; }
        public string RelTable { get; set; }
        public string AppType { get; set; }
        public string Files { get; set; }
        public int? ComId { get; set; }
        public int? ORDERID { get; set; }
        public int? IsSys { get; set; }
        public string WXUrl { get; set; }
        public int? IsKJFS { get; set; }
        public string PModelCode { get; set; }
        public string Token { get; set; }
        public string EncodingAESKey { get; set; }
    }
}
