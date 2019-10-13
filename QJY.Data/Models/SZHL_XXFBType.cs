using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_XXFBType
    {
        public int ID { get; set; }
        public int? ComId { get; set; }
        public string TypeName { get; set; }
        public int? PTypeID { get; set; }
        public string TypeDec { get; set; }
        public string TypeManager { get; set; }
        public string CheckUser { get; set; }
        public string IsCheck { get; set; }
        public string TypePath { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public int? IsDel { get; set; }
        public string IsSee { get; set; }
        public string SeeUser { get; set; }
        public string ISzjfb { get; set; }
    }
}
