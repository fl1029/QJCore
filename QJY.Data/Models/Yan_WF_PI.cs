using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class Yan_WF_PI
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string Description { get; set; }
        public int? PDID { get; set; }
        public DateTime? StartTime { get; set; }
        public int? StartTaskInstanceID { get; set; }
        public string RelatedTable { get; set; }
        public string WFFormNum { get; set; }
        public string isComplete { get; set; }
        public DateTime? CompleteTime { get; set; }
        public string IsSuspended { get; set; }
        public DateTime? SuspendedTime { get; set; }
        public string IsCanceled { get; set; }
        public DateTime? CanceledTime { get; set; }
        public string CanceledReason { get; set; }
        public string Remark1 { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string PITYPE { get; set; }
        public string ChaoSongUser { get; set; }
        public string CRUserName { get; set; }
        public int? BranchNO { get; set; }
        public string BranchName { get; set; }
        public string Content { get; set; }
        public string isGD { get; set; }
        public DateTime? GDDate { get; set; }
    }
}
