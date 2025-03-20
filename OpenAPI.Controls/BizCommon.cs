using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Controls
{
    public static class BizCommon
    {
        public static DateTime StringtoDateTime(string TM, string format)
        {
            if (DateTime.TryParseExact(TM, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            else
            {
                throw new FormatException("Invalid format. Expected yyyyMMddHH.");
            }
        }

        public static DateTime StringtoDateTime_24Correction(string TM, string format)
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

        public static string ConvertToNextDayMidnight(string obsdh)
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

        public static DateTime StringtoDateTime(string TM)
        {
            int year = int.Parse(TM.Substring(0, 4));
            int month = int.Parse(TM.Substring(4, 2));
            int day = int.Parse(TM.Substring(6, 2));
            int hour = int.Parse(TM.Substring(8, 2));
            int min = int.Parse(TM.Substring(10, 2));

            return new DateTime(year, month, day, hour, min, 0);
        }

        public static DateTime StringtoDateTimeStart(string TM)
        {
            int year = int.Parse(TM.Substring(0, 4));
            int month = int.Parse(TM.Substring(4, 2));
            int day = int.Parse(TM.Substring(6, 2));

            return new DateTime(year, month, day, 0, 0, 0);
        }

        public static DateTime StringtoDateTimeEnd(string TM)
        {
            int year = int.Parse(TM.Substring(0, 4));
            int month = int.Parse(TM.Substring(4, 2));
            int day = int.Parse(TM.Substring(6, 2));

            return new DateTime(year, month, day, 23, 59, 59);
        }

        public static bool BoolConvert(string realTimeUse)
        {
            if (bool.TryParse(realTimeUse, out bool result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }

        public static List<string> ConvertDataTableTostringList<T>(DataTable dt)
        {
            List<string> list = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                string addData = row["sgg_cd"].ToString();

                list.Add(addData);
            }

            return list;
        }

        public static List<T> ConvertDataTableToList<T>(DataTable dt) where T : new()
        {
            List<T> list = new List<T>();
            var properties = typeof(T).GetProperties();

            foreach (DataRow row in dt.Rows)
            {
                T obj = new T();
                foreach (var prop in properties)
                {
                    if (dt.Columns.Contains(prop.Name))
                    {
                        object value = row[prop.Name];

                        // int 타입이면 변환
                        if (prop.PropertyType == typeof(int))
                        {
                            value = int.TryParse(value.ToString(), out int intValue) ? intValue : 0;
                        }
                        else if (prop.PropertyType == typeof(double))
                        {
                            value = double.TryParse(value.ToString(), out double doubleValue) ? doubleValue : 0.0;
                        }

                        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
                list.Add(obj);
            }

            return list;
        }

        public static int GetTotalDays(int year)
        {
            int totalDays = 0;

            for (int month = 1; month <= 12; month++)
            {
                totalDays += DateTime.DaysInMonth(year, month);
            }

            return totalDays;
        }

        
    }
}
