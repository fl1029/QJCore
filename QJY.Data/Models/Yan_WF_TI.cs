using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class Yan_WF_TI
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string TDCODE { get; set; }
        public int PIID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TaskUserID { get; set; }
        public string TaskUserView { get; set; }
        public string zhuandanfrom { get; set; }
        public string zhuandanto { get; set; }
        public string NextTaskInstance { get; set; }
        public string PreTaskInstance { get; set; }
        public int? TaskState { get; set; }
        public int? ChiTaskCount { get; set; }
        public string ChiTaskType { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string TaskName { get; set; }
        public string TaskRole { get; set; }
    }
}
