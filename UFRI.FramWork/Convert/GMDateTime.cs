/////////////////////////////////////////////////////////////////////////////////////
/// ◑ Solution 		: UFRI
/// ◑ Project			: UFRI.FrameWork
/// ◑ Class Name		: EzDateTimeFormat
/// ◑ Description		: 날자형 지원 클래스
/// 
/// ◑ Revision History
/////////////////////////////////////////////////////////////////////////////////////
/// Date			Author		    Description
/////////////////////////////////////////////////////////////////////////////////////
/// 2017/12/27      GiMoon     First Draft
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    /// <summary>
    /// EzDateTime 클래스에서 사용하기 위한 Format 종류 미리 정의
    /// </summary>
    public enum GMDateTimeFormat
    {
        /// <summary>2007-02-19</summary>
        yyyyMMdd,
        /// <summary>
        /// 2010-02-03 23시
        /// </summary>
        yyyyMMddHH,
        /// <summary>
        /// 20070205
        /// </summary>
        yyyyMMddNonSeperator,
        /// <summary>2007-2-19</summary>
        yyyyMd,
        /// <summary>201001</summary>
        yyyyMM,
        /// <summary>02-19-2007</summary>
        MMddyyyy,
        /// <summary>2-19-2007</summary>
        Mdyyyy,
        /// <summary>19-02-2007</summary>
        ddMMyyyy,
        /// <summary>19-2-2007</summary>
        dMyyyy,
        /// <summary>2007-02-19 18:44:53</summary>
        yyyyMMddHHmmss,
        /// <summary>2007-2-19 18:44:53</summary>
        yyyyMdHHmmss,
        /// <summary>02-19-2007 18:44:53</summary>
        MMddyyyyHHmmss,
        /// <summary>2-19-2007 18:44:53</summary>
        MdyyyyHHmmss,
        /// <summary>19-02-2007 18:44:53</summary>
        ddMMyyyyHHmmss,
        /// <summary>19-2-2007 18:44:53</summary>
        dMyyyyHHmmss,
        /// <summary>
        /// 01/09
        /// </summary>
        MMdd,
    }

    public class GMDateTime
    {
        /// <summary>
        /// 생성을 막고, 상속이 가능하게 하기 위한 생성자
        /// </summary>
        protected GMDateTime()
        {
        }

        #region GetDayNames - 요일명 배열
        /// <summary>
        /// 현재 Culture의 요일의 culture 관련 전체 이름이 들어있는 배열을 반환
        /// </summary>
        /// <returns>일요일,월요일,...,토요일</returns>
        public static string[] GetDayNames()
        {
            return GetDayNames(CultureInfo.CurrentCulture.ToString());
        }
        /// <summary>
        /// 지정된 Culture의 요일의 culture 관련 전체 이름이 들어있는 배열을 반환
        /// </summary>
        /// <returns>일요일,월요일,...,토요일</returns>
        public static string[] GetDayNames(string culture)
        {
            return new CultureInfo(culture, false).DateTimeFormat.DayNames;
        }
        /// <summary>
        /// 지정된 Culture의 요일의 culture 관련 전체 이름이 들어있는 배열을 반환
        /// </summary>
        /// <returns>일요일,월요일,...,토요일</returns>
        public static string[] GetDayNames(CultureInfo culture)
        {
            return culture.DateTimeFormat.DayNames;
        }
        #endregion

        #region GetMonthNames - 월이름 배열
        /// <summary>
        /// 현재 Culture의 월의 culture 관련 전체 이름이 들어있는 배열을 반환
        /// </summary>
        /// <returns>"1월","2월",...,"12월",""</returns>
        public static string[] GetMonthNames()
        {
            return GetMonthNames(CultureInfo.CurrentCulture.ToString());
        }
        /// <summary>
        /// 지정된 Culture의 월의 culture 관련 전체 이름이 들어있는 배열을 반환
        /// </summary>
        /// <returns>"1월","2월",...,"12월",""</returns>
        public static string[] GetMonthNames(string culture)
        {
            return new CultureInfo(culture, false).DateTimeFormat.MonthNames;
        }
        /// <summary>
        /// 지정된 Culture의 월의 culture 관련 전체 이름이 들어있는 배열을 반환
        /// </summary>
        /// <returns>"1월","2월",...,"12월",""</returns>
        public static string[] GetMonthNames(CultureInfo culture)
        {
            return culture.DateTimeFormat.MonthNames;
        }
        #endregion

        #region DayNameNow - 현재 요일명
        /// <summary>
        /// 지정된 culture에 기반한 현재 요일의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>화요일,...</returns>
        public static string DayNameNow()
        {
            return ToDayName(DateTime.Now, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정된 culture에 기반한 현재 요일의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>화요일,...</returns>
        public static string DayNameNow(string culture)
        {
            return ToDayName(DateTime.Now, culture);
        }
        /// <summary>
        /// 지정된 culture에 기반한 현재 요일의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>화요일,...</returns>
        public static string DayNameNow(CultureInfo culture)
        {
            return ToDayName(DateTime.Now, culture);
        }
        #endregion

        #region MonthNameNow - 현재 월이름
        /// <summary>
        /// 지정된 culture에 기반한 현재 월의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>N월,...</returns>
        public static string MonthNameNow()
        {
            return ToMonthName(DateTime.Now, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정된 culture에 기반한 현재 월의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>N월,...</returns>
        public static string MonthNameNow(string culture)
        {
            return ToMonthName(DateTime.Now, culture);
        }
        /// <summary>
        /// 지정된 culture에 기반한 현재 월의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>N월,...</returns>
        public static string MonthNameNow(CultureInfo culture)
        {
            return ToMonthName(DateTime.Now, culture);
        }
        #endregion

        #region WeekOfYearNow - 현재 년에서의 주차
        /// <summary>
        /// 현재 culture의 주시작요일을 기준으로 오늘가 해당해에서 몇번째 주에 속하는지 반환
        /// </summary>
        public static int WeekOfYearNow()
        {
            return ToWeekOfYear(DateTime.Now, CultureInfo.CurrentCulture);
        }
        #endregion

        #region ToDayName - 요일명 구하기
        /// <summary>
        /// 현재 culture에 기반한 지정된 요일의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>화요일,...</returns>
        public static string ToDayName(DateTime date)
        {
            return ToDayName(date, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정된 culture에 기반한 지정된 요일의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>화요일,...</returns>
        public static string ToDayName(DateTime date, string culture)
        {
            return new CultureInfo(culture, false).DateTimeFormat.GetDayName(date.DayOfWeek);
        }
        /// <summary>
        /// 지정된 culture에 기반한 지정된 요일의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>화요일,...</returns>
        public static string ToDayName(DateTime date, CultureInfo culture)
        {
            return culture.DateTimeFormat.GetDayName(date.DayOfWeek);
        }
        #endregion

        #region ToMonthName - 월이름 구하기
        /// <summary>
        /// 현재 culture에 기반한 지정된 월의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>N월,...</returns>
        public static string ToMonthName(DateTime date)
        {
            return ToMonthName(date, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정된 culture에 기반한 지정된 월의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>N월,...</returns>
        public static string ToMonthName(DateTime date, string culture)
        {
            return new CultureInfo(culture, false).DateTimeFormat.GetMonthName(date.Month);
        }
        /// <summary>
        /// 지정된 culture에 기반한 지정된 월의 culture 관련 전체 이름을 반환
        /// </summary>
        /// <returns>N월,...</returns>
        public static string ToMonthName(DateTime date, CultureInfo culture)
        {
            return culture.DateTimeFormat.GetMonthName(date.Month);
        }
        #endregion

        #region ToWeekOfYear - 년에서의 주차
        /// <summary>
        /// 현재 culture의 주시작요일을 기준으로 지정된 날짜가 해당해에서 몇번째 주에 속하는지 반환
        /// </summary>
        public static int ToWeekOfYear(DateTime dt)
        {
            return ToWeekOfYear(dt, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정한 culture의 주시작요일을 기준으로 지정된 날짜가 해당해에서 몇번째 주에 속하는지 반환
        /// </summary>
        public static int ToWeekOfYear(DateTime dt, string culture)
        {
            return ToWeekOfYear(dt, new CultureInfo(culture, false));
        }
        /// <summary>
        /// 지정한 culture의 주시작요일을 기준으로 지정된 날짜가 해당해에서 몇번째 주에 속하는지 반환
        /// </summary>
        public static int ToWeekOfYear(DateTime dt, CultureInfo culture)
        {
            return culture.DateTimeFormat.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        }
        #endregion

        #region GetFirstDayInMonth, GetLastDayInMonth - 월의 시작일, 종료일
        /// <summary>
        /// 지정한 날짜가 속한 월의 첫 날을 획득
        /// </summary>
        public static DateTime GetFirstDayInMonth(DateTime dtDate)
        {
            return new DateTime(dtDate.Year, dtDate.Month, 1);
        }
        /// <summary>
        /// 지정한 년월의 첫 날을 획득
        /// </summary>
        public static DateTime GetFirstDayInMonth(int year, int month)
        {
            return new DateTime(year, month, 1);
        }
        /// <summary>
        /// 지정한 날짜가 속한 월의 마지막 날을 획득
        /// </summary>
        public static DateTime GetLastDayInMonth(DateTime dtDate)
        {
            return new DateTime(dtDate.Year, dtDate.Month, DateTime.DaysInMonth(dtDate.Year, dtDate.Month));
        }
        /// <summary>
        /// 지정한 년월의 마지막 날을 획득
        /// </summary>
        public static DateTime GetLastDayInMonth(int year, int month)
        {
            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }
        #endregion

        #region ToDateTimeString - EzDateTimeFormat 커스텀 형식으로 포맷팅
        /// <summary>
        /// 지정된 날짜를 현재 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
        /// </summary>
        public static string ToDateTimeString(string date, GMDateTimeFormat format)
        {
            return ToDateTimeString(Convert.ToDateTime(date), format, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정된 날짜를 현재 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
        /// </summary>
        public static string ToDateTimeString(DateTime date, GMDateTimeFormat format)
        {
            return ToDateTimeString(date, format, CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 지정된 날짜를 지정된 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
        /// </summary>
        public static string ToDateTimeString(DateTime date, GMDateTimeFormat format, string culture)
        {
            return ToDateTimeString(date, format, new CultureInfo(culture, false));
        }
        /// <summary>
        /// 지정된 날짜를 지정된 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
        /// </summary>
        public static string ToDateTimeString(DateTime date, GMDateTimeFormat format, CultureInfo culture)
        {
            DateTime dt = DateTime.MinValue;
            //if (date != null && Convert.IsDBNull(date))
            if (date != null)
            {
                dt = date;
            }

            if (dt > DateTime.MinValue)
            {
                return culture.DateTimeFormat.Calendar.ToDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond).ToString(GetDateTimeFormatString(format));
            }
            return string.Empty;
        }
        /// <summary>
        /// 오전/오후 제거하고 순수 날짜 포맷 가져오기
        /// </summary>
        /// <param name="dtCurrentDate"></param>
        /// <returns></returns>
        public static String GetPureDateTimeFormat(DateTime dtCurrentDate)
        {
            return GMConvert.ToString(dtCurrentDate).Replace("오전", "").Replace("오후", "");
        }
        /// <summary>
        /// 단일 포맷 형식의 문자형 날짜 데이터를 날짜 포맷으로 변경
        /// </summary>
        /// <param name="sDate">YYYYMMDDHHMMSS => DateTime Format</param>
        /// <returns></returns>
        public static DateTime SingleFormatStringToDateTime(String sDate)
        {
            DateTime dtResult = DateTime.Now;

            String sYear = sDate.Substring(0, 4);
            String sMonth = sDate.Substring(4, 2);
            String sDay = sDate.Substring(6, 2);
            String sHour = sDate.Substring(8, 2);
            String sMin = sDate.Substring(10, 2);
            String sSec = sDate.Substring(12, 2);

            String sDateTime = sYear + "-" + sMonth + "-" + sDay + " " + sHour + ":" + sMin + ":" + sSec;

            dtResult = Convert.ToDateTime(sDateTime);

            return dtResult;
        }
        /// <summary>
        /// 지정된 형식으로 DateTime형식을 포매팅 하기위한 클래스 (null이면 String.Empty)
        /// </summary>
        private static string GetDateTimeFormatString(GMDateTimeFormat format)
        {
            switch (format)
            {
                case GMDateTimeFormat.yyyyMMdd:
                    return "yyyy-MM-dd";
                case GMDateTimeFormat.yyyyMMddHHmmss:
                    return "yyyyMMddHHmmss";
                case GMDateTimeFormat.yyyyMMddHH:
                    return "yyyy-MM-dd HH시";

                case GMDateTimeFormat.yyyyMd:
                    return "yyyy-M-d";
                case GMDateTimeFormat.MMddyyyy:
                    return "MM-dd-yyyy";

                case GMDateTimeFormat.Mdyyyy:
                    return "M-d-yyyy";

                case GMDateTimeFormat.ddMMyyyy:
                    return "dd-MM-yyyy";

                case GMDateTimeFormat.dMyyyy:
                    return "d-M-yyyy";

                case GMDateTimeFormat.yyyyMdHHmmss:
                    return "yyyy-M-d HH:mm:ss";

                case GMDateTimeFormat.MMddyyyyHHmmss:
                    return "MM-dd-yyyy HH:mm:ss";

                case GMDateTimeFormat.MdyyyyHHmmss:
                    return "M-d-yyyy HH:mm:ss";

                case GMDateTimeFormat.ddMMyyyyHHmmss:
                    return "dd-MM-yyyy HH:mm:ss";

                case GMDateTimeFormat.dMyyyyHHmmss:
                    return "d-M-yyyy HH:mm:ss";
                case GMDateTimeFormat.yyyyMM:
                    return "yyyyMM";
                case GMDateTimeFormat.yyyyMMddNonSeperator:
                    return "yyyyMMdd";
                case GMDateTimeFormat.MMdd:
                    return "MM/dd";
            }
            return "yyyy-MM-dd HH:mm:ss";
        }
        #endregion

        #region ToFullDateTimeString -  ShortDateString + ShortTimeString (yyyy-MM-dd HH:mm:ss) 형식
        /// <summary>
        /// 현재시간을 현재 culture의 ShortDatePattern형식에 HH:mm:ss형식을 더한 형식 반환
        /// 한국 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <returns>한국은 yyyy-MM-dd HH:mm:ss</returns>
        public static string ToFullDateTimeString()
        {
            return ToFullDateTimeString(CultureInfo.CurrentCulture.ToString());
        }
        /// <summary>
        /// 현재시간을 지정된 culture의 ShortDatePattern형식에 HH:mm:ss형식을 더한 형식 반환
        /// 한국 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <returns>한국은 yyyy-MM-dd HH:mm:ss</returns>
        public static string ToFullDateTimeString(string culture)
        {
            DateTimeFormatInfo info = new CultureInfo(culture, false).DateTimeFormat;
            return DateTime.Now.ToString(string.Format("{0} {1}", info.ShortDatePattern, "HH:mm:ss"));
        }
        #endregion

        #region ToLongDateString -  LongDatePattern(yyyy년 M월 d일 *요일) 형식
        /// <summary>
        /// 현재시간을 현재 culture의 LongDatePattern형식 반환
        /// </summary>
        /// <returns>yyyy년 M월 d일 *요일</returns>
        public static string ToLongDateString()
        {
            return ToLongDateString(CultureInfo.CurrentCulture.ToString());
        }
        /// <summary>
        /// 현재시간을 지정된 culture의 LongDatePattern형식 반환
        /// </summary>
        /// <returns>yyyy년 M월 d일 *요일</returns>
        public static string ToLongDateString(string culture)
        {
            return ToLongDateString(new CultureInfo(culture, false));
        }
        /// <summary>
        /// 현재시간을 지정된 culture의 LongDatePattern형식 반환
        /// </summary>
        /// <returns>yyyy년 M월 d일 *요일</returns>
        public static string ToLongDateString(CultureInfo culture)
        {
            DateTimeFormatInfo info = culture.DateTimeFormat;
            return DateTime.Now.ToString(info.LongDatePattern);
        }
        #endregion

        #region ToLongTimeString - LongTimePattern(한국 - 오후 H:mm:ss) 형식
        /// <summary>
        /// 현재시간을 현재 culture의 LongTimePattern형식 반환
        /// </summary>
        /// <returns>한국 - 오후 H:mm:ss</returns>
        public static string ToLongTimeString()
        {
            return ToLongTimeString(CultureInfo.CurrentCulture.ToString());
        }
        /// <summary>
        /// 현재시간을 현재 culture의 LongTimePattern형식 반환
        /// </summary>
        /// <returns>한국 - 오후 H:mm:ss</returns>
        public static string ToLongTimeString(string culture)
        {
            return ToLongTimeString(new CultureInfo(culture, false));
        }
        /// <summary>
        /// 현재시간을 현재 culture의 LongTimePattern형식 반환
        /// </summary>
        /// <returns>한국 - 오후 H:mm:ss</returns>
        public static string ToLongTimeString(CultureInfo culture)
        {
            DateTimeFormatInfo info = culture.DateTimeFormat;
            return DateTime.Now.ToString(info.LongTimePattern);
        }
        #endregion

        #region [ LocalDateTime , UTC DateTime ]

        //public static DateTime[] GetLocalDateTimeAndGetUTCDateTime()
        //{
            //String sLocalDateTime = ConstFactory.GetCurrentDateTime;
            //DateTime dtLocalDateTime = GMConvert.ToDateTime(sLocalDateTime);
            //DateTime dtUTCDateTime = GMDateTime.ConvertToLocalDateTimeToUTCTime(dtLocalDateTime);
            //sLocalDateTime = dtLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            //String sUTCDateTime = dtUTCDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            //DateTime[] dtResult = new DateTime[2];

            //dtResult[0] = dtLocalDateTime;
            //dtResult[1] = dtUTCDateTime;

            //return dtResult;
        //}

        /// <summary>
        /// DateTime.Now.ToLocalTime
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLocalTime()
        {
            return DateTime.Now.ToLocalTime();
        }
        /// <summary>
        /// DateTime.Now.ToLocalTime().ToUniversalTime
        /// </summary>
        /// <returns></returns>
        public static DateTime GetUTCTime()
        {
            return DateTime.Now.ToLocalTime().ToUniversalTime();
        }
        /// <summary>
        /// Local Date Time To UTC DateTime
        /// </summary>
        /// <param name="dtLocalDateTime">YYYY-MM-DD HH:MM:SS => UTC Time</param>
        /// <returns></returns>
        public static DateTime ConvertToLocalDateTimeToUTCTime(DateTime dtLocalDateTime)
        {
            return dtLocalDateTime.ToUniversalTime();
        }
        #endregion

        #region ToShortDateString - 한국 yyyy-MM-dd 형식
        /// <summary>
        /// 현재시간을 현재 culture의 ShortDatePattern형식 반환
        /// 한국 yyyy-MM-dd
        /// </summary>
        /// <returns>한국은 yyyy-MM-dd</returns>
        public static string ToShortDateString()
        {
            return ToShortDateString(CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// 현재시간을 지정된 culture의 ShortDatePattern형식 반환
        /// 한국 yyyy-MM-dd
        /// </summary>
        /// <returns>한국은 yyyy-MM-dd</returns>
        public static string ToShortDateString(string culture)
        {
            return ToShortDateString(new CultureInfo(culture, false));
        }
        /// <summary>
        /// 현재시간을 지정된 culture의 ShortDatePattern형식 반환
        /// 한국 yyyy-MM-dd
        /// </summary>
        /// <returns>한국은 yyyy-MM-dd</returns>
        public static string ToShortDateString(CultureInfo culture)
        {
            DateTimeFormatInfo info = culture.DateTimeFormat;
            return DateTime.Now.ToString(info.ShortDatePattern);
        }
        #endregion

        #region ToShortTimeString - HH:mm:ss 형식
        /// <summary>
        /// 현재시간을 현재 culture의 HH:mm:ss형식반환
        /// 원래 ShortTimePattern은 - 오전 H:mm 형식인데 여기서는 Custom구현
        /// HH:mm:ss
        /// </summary>
        /// <returns>HH:mm:ss</returns>
        public static string ToShrotTimeString()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

        #region GetTimeSpan
        /// <summary>
        /// 두시간사이의 차이 반환. TimeSpan의 절대값 Duration
        /// </summary>
        public static TimeSpan GetTimeSpan(DateTime dt1, DateTime dt2)
        {
            return dt2.Subtract(dt1).Duration();
        }
        /// <summary>
        /// 두시간사이의 차이 반환. TimeSpan의 절대값 Duration
        /// </summary>
        public static TimeSpan GetTimeSpan(string dt1, string dt2)
        {
            DateTime time = Convert.ToDateTime(dt1);
            DateTime time2 = Convert.ToDateTime(dt2);
            return GetTimeSpan(time, time2);
        }
        #endregion
    }
}
