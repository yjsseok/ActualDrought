using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class ReservoirLevelData
    {
        public string check_date { get; set; }    // 조회 날짜
        public string county { get; set; }        // 지역(시군구)
        public string fac_code { get; set; }      // 저수지 코드
        public string fac_name { get; set; }      // 저수지 이름
        public string rate { get; set; }          // 저수율(%)
    }
}
