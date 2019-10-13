using System;
using System.Collections.Generic;

namespace QJY.Data
{
    public partial class SZHL_YCGL_CAR
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string CarNum { get; set; }
        public string CarType { get; set; }
        public string CarBrand { get; set; }
        public string CarXH { get; set; }
        public string Files { get; set; }
        public string Status { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
        public int? intProcessStanceid { get; set; }
        public string BuyDate { get; set; }
        public string Price { get; set; }
        public string FDJNum { get; set; }
        public string SeatNum { get; set; }
        public string Remark { get; set; }
    }
}
