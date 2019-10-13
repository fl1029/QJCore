using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class JH_Auth_Role
    {
        public int? ComId { get; set; }
        public int RoleCode { get; set; }
        public string RoleName { get; set; }
        public string RoleDec { get; set; }
        public int PRoleCode { get; set; }
        public int DisplayOrder { get; set; }
        public int leve { get; set; }
        public string IsUse { get; set; }
        public string isSysRole { get; set; }
        public int? WXBQCode { get; set; }
        public string RoleQX { get; set; }
        public string IsHasQX { get; set; }
    }
}
