using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_DBGL
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public string Remark { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
