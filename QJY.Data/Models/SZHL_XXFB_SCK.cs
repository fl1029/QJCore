using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_XXFB_SCK
    {
        public int Id { get; set; }
        public int? ComId { get; set; }
        public int? Type { get; set; }
        public string Files { get; set; }
        public string Content { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
