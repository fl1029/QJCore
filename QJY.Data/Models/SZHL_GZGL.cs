using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_GZGL
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string BranchName { get; set; }
        public string UserName { get; set; }
        public string UserRealName { get; set; }
        public decimal? JBGZ { get; set; }
        public decimal? GWJT { get; set; }
        public decimal? JX { get; set; }
        public decimal? FL { get; set; }
        public decimal? KK { get; set; }
        public decimal? YFGZ { get; set; }
        public decimal? SFGZ { get; set; }
        public decimal? GRJN { get; set; }
        public decimal? GSJN { get; set; }
        public string Remark { get; set; }
        public int? Status { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
