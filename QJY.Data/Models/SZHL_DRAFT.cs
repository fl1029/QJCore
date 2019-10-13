using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_DRAFT
    {
        public int ID { get; set; }
        public int? ComId { get; set; }
        public string FormCode { get; set; }
        public string FormID { get; set; }
        public string JsonData { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRTime { get; set; }
        public string ExtData { get; set; }
        public int? DataID { get; set; }
    }
}
