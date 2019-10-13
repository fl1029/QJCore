using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Branch
    {
        public int? ComId { get; set; }
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public int DeptShort { get; set; }
        public string DeptDesc { get; set; }
        public int DeptRoot { get; set; }
        public int BranchLevel { get; set; }
        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public string BranchLeader { get; set; }
        public int? WXBMCode { get; set; }
        public string TXLQX { get; set; }
        public string RoleQX { get; set; }
        public string IsHasQX { get; set; }
        public string RoomCode { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}
