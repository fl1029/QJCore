using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class Yan_WF_PD
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string ProcessName { get; set; }
        public string RelatedTable { get; set; }
        public string Alias { get; set; }
        public string Poption { get; set; }
        public string ProcessType { get; set; }
        public string IsDefineCompleted { get; set; }
        public string IsSuspended { get; set; }
        public int? DueDate { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUserName { get; set; }
        public int? lcstatus { get; set; }
        public string isTemp { get; set; }
        public string Tempcontent { get; set; }
        public string ChaoSongUser { get; set; }
        public string ManageUser { get; set; }
        public string ManageRole { get; set; }
        public string ProcessClass { get; set; }
        public string Files { get; set; }
        public string ExportFile { get; set; }
        public string CRUser { get; set; }
        public string SQUser { get; set; }
        public string fmdata { get; set; }
        public string gltable { get; set; }
        public string gltable1 { get; set; }
        public string isgltable { get; set; }
    }
}
