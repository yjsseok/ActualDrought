using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class KMASiteInformation
    {
        /// <summary>
        /// 지점번호
        /// </summary>
        public int STD_ID { get; set; }

        /// <summary>
        /// 경도(degree)
        /// </summary>
        public double LON { get; set; }
        /// <summary>
        /// 위도(degree)
        /// </summary>
        public double LAT { get; set; }
        /// <summary>
        /// 지점 특성코드
        /// </summary>
        public string STN_SP { get; set; }
        /// <summary>
        /// 노장 해발고도(m)
        /// </summary>
        public double HT { get; set; }
        /// <summary>
        /// 기압계 해발고도 (m)
        /// </summary>
        public double HT_PA { get; set; }
        /// <summary>
        /// 온도계 지상높이 (m)
        /// </summary>
        public double HT_TA { get; set; }
        /// <summary>
        /// 풍향/풍속계 지상높이(m)
        /// </summary>
        public double HT_WD { get; set; }
        /// <summary>
        /// 우량계 지상높이 (m)
        /// </summary>
        public double HT_RN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int LAU_ID { get; set; }
        /// <summary>
        /// 관리관서번호
        /// </summary>
        public int STN_AD { get; set; }
        /// <summary>
        /// 지점명 (한글)
        /// </summary>
        public string STN_KO { get; set; }
        /// <summary>
        /// 지점명 (영문)
        /// </summary>
        public string STN_EN { get; set; }
        /// <summary>
        /// 예보구역코드
        /// </summary>
        public string FCT_ID { get; set; }
        /// <summary>
        /// 법정동코드
        /// </summary>
        public string LAW_ID { get; set; }
        /// <summary>
        /// 수계코드
        /// </summary>
        public int BASIN { get; set; }
    }
}
