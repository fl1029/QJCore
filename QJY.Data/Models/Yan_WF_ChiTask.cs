using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class Yan_WF_ChiTask
    {
        public string ComId { get; set; }
        public int ID { get; set; }
        public string TaskInstanceID { get; set; }
        public string TaskUser { get; set; }
        public DateTime? TaskEndDate { get; set; }
        public DateTime? Crdate { get; set; }
        public string CrUser { get; set; }
        public string IsHasManager { get; set; }
        public string UserView { get; set; }
        public string Remark { get; set; }
        public string isCounterSign { get; set; }
    }
}
