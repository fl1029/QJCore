using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_DXGL
    {
        public string dxContent { get; set; }
        public string dxnums { get; set; }
        public DateTime? SendTime { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string isSend { get; set; }
        public string Remark { get; set; }
        public int ID { get; set; }
        public int ComId { get; set; }
    }
}
