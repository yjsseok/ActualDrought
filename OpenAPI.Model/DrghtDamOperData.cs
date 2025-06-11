using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class DrghtDamOperData
    {
        public string damcd { get; set; }    // 댐 코드
        public string damnm { get; set; }    // 댐 이름
        public double iqty { get; set; }     // 유입량
        public double lwl { get; set; }      // 저수위
        public string obsymd { get; set; }   // 관측일자
        public double rsqty { get; set; }    // 저수량
        public double rsrt { get; set; }     // 저수율
    }

}
