using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_XZ_JL
    {
        public int ID { get; set; }
        public string YearMonth { get; set; }
        public string title { get; set; }
        public string rise { get; set; }
        public string Inscribe { get; set; }
        public string salaryData { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public int? ComId { get; set; }
    }
}
