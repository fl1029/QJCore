using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class Yan_WF_TD
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string TaskName { get; set; }
        public int? ProcessDefinitionID { get; set; }
        public int? Taskorder { get; set; }
        public string pretask { get; set; }
        public string NodeType { get; set; }
        public string ProcessLogic { get; set; }
        public string AssignedRole { get; set; }
        public string TaskAssInfo { get; set; }
        public string AboutAttached { get; set; }
        public string ReadableFields { get; set; }
        public string WritableFields { get; set; }
        public int? DueDate { get; set; }
        public string AssignedUsers { get; set; }
        public string isforbid { get; set; }
        public string IsDetailUser { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public string isSJ { get; set; }
        public string TDCODE { get; set; }
        public string isCanEdit { get; set; }
    }
}
