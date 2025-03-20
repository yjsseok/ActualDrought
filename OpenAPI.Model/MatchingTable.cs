using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class MatchingTable
    {
        public string RegionCode { get; set; }  // 시군코드
        public string DataType { get; set; } // 데이터종류
        public string MatchKey { get; set; } // 매칭키
    }
}
