using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_NOTE
    {
        public int ID { get; set; }
        public string NoteContent { get; set; }
        public DateTime? CRDate { get; set; }
        public string CRUser { get; set; }
        public int? LeiBie { get; set; }
        public string UPUser { get; set; }
        public DateTime? UPDDate { get; set; }
        public string NoteTitle { get; set; }
        public int? ComId { get; set; }
    }
}
