using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_GZGL_JCSZ
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string UserName { get; set; }
        public decimal? JBGZ { get; set; }
        public decimal? GWJT { get; set; }
        public int? IsSB { get; set; }
        public int? IsGJJ { get; set; }
        public string Files { get; set; }
        public string Remark { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
