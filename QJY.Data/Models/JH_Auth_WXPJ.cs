using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_WXPJ
    {
        public int ID { get; set; }
        public string TJID { get; set; }
        public string TJSecret { get; set; }
        public string Token { get; set; }
        public string EncodingAESKey { get; set; }
        public string Ticket { get; set; }
        public string Remark { get; set; }
        public DateTime? CreateTime { get; set; }
        public string TJName { get; set; }
        public string TJStatus { get; set; }
    }
}
