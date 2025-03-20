using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Model
{
    public class DamHRData
    {
        /// <summary>
        /// 댐코드
        /// </summary>
        public string damcd {  get; set; }
        /// <summary>
        /// 관측일시
        /// </summary>
        public string obsdh { get; set; }

        /// <summary>
        /// 저수위(El.m)
        /// </summary>
        public string rwl { get; set; }

        /// <summary>
        /// 방수로수위(El.m)
        /// </summary>
        public string ospilwl { get; set; }

        /// <summary>
        /// 저수량(M㎥)
        /// </summary>
        public string rsqty { get; set; }

        /// <summary>
        /// 저수율(%)
        /// </summary>
        public string rsrt { get; set; }

        /// <summary>
        /// 유입량(㎥/s)
        /// </summary>
        public string iqty { get; set; }

        /// <summary>
        /// 공용량(백만㎥)
        /// </summary>
        public string etqty { get; set; }

        /// <summary>
        /// 총 방류량(㎥/s)
        /// </summary>
        public string tdqty { get; set; }

        /// <summary>
        /// 발전방류량(㎥/s)
        /// </summary>
        public string edqty { get; set; }

        /// <summary>
        /// 여수로방류량(㎥/s)	
        /// </summary>
        public string spdqty { get; set; }

        /// <summary>
        /// 기타방류량(㎥/s)	
        /// </summary>
        public string otltdqty { get; set; }

        /// <summary>
        /// 취수량(㎥/s)	
        /// </summary>
        public string itqty { get; set; }

        /// <summary>
        /// 댐유역평균우량(mm)	
        /// </summary>
        public string dambsarf { get; set; }

        // Rwl을 double 형식으로 변환
        public double? GetRwlAsDouble()
        {
            if (double.TryParse(rwl, out double rwlValue))
            {
                return rwlValue;
            }
            return null; // 변환 실패 시 null 반환
        }

        public DateTime GetObservationDateTime()
        {
            return StringtoDateTime(obsdh, "yyyyMMddHH");
            //return DateTime.ParseExact(newDate, "yyyyMMddHH", CultureInfo.InvariantCulture);
        }

        public DateTime StringtoDateTime(string TM, string format)
        {
            //시간추출
            string Hr = TM.Substring(8, 2);
            string NewDate = string.Empty;

            //24경우 다음날 00시로 변경
            if (Hr == "24")
            {
                NewDate = ConvertToNextDayMidnight(TM);
            }
            else
            {
                NewDate = TM;
            }

            if (DateTime.TryParseExact(NewDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            else
            {
                throw new FormatException("Invalid format. Expected yyyyMMddHH.");
            }
        }

        public string ConvertToNextDayMidnight(string obsdh)
        {
            if (!DateTime.TryParseExact(obsdh.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                throw new FormatException("Invalid date format.");
            }

            // 다음 날로 변경
            date = date.AddDays(1);

            // 변환된 날짜 + "00"을 반환
            return date.ToString("yyyyMMdd") + "00";
        }
    }
}
