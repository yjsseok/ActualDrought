using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public class GMConvert : GMDateTime
    {
        /// <summary>
        /// 생성을 막고, 상속이 가능하게 하기 위한 생성자
        /// </summary>
        protected GMConvert()
        {
        }

        #region 언어관련
        /// <summary>
        /// Int를 상속받은 열거형의 숫자값을 문자열로 반환 (기본적인 ToString()은 열거형값의 문자열을 그대로 반환한다.)
        /// </summary>
        /// <param name="enumValue">Int를 상속받은 열거형값</param>
        /// <returns>열거형값 문자열 : "1"...</returns>
        public static string EnumToIntString(object enumValue)
        {
            if (enumValue.GetType().BaseType == null || enumValue.GetType().BaseType.FullName != "System.Enum")
            {
                throw new ArgumentException("파라미터값이 열거형이 아닙니다.(Enum필요)", "enumValue");
            }
            return ((int)enumValue).ToString();
        }

        /// <summary>
        /// Where절의 Like조건으로 적용할 수 있도록 "%"기호 처리
        /// </summary>
        /// <param name="text">%를 포함하지 않는 Like문자열</param>
        /// <returns>%처리된 조건절 값</returns>
        public static string ToLikeClause(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "%";
            }
            return "%" + text + "%";
        }
        #endregion

        #region null, DBNull, EmptyString 체크
        /// <summary>
        /// DBNull 또는 null인지 검사
        /// </summary>
        public static bool IsNull(object val)
        {
            return (val == null || Convert.IsDBNull(val));
        }
        /// <summary>
        /// DBNull 또는 null인지 검사
        /// </summary>
        public static bool IsNullOrEmpty(object val)
        {
            return (val == null || Convert.IsDBNull(val) || string.Format("{0}", val) == string.Empty);
        }
        /// <summary>
        /// DBNull 또는 null이 아닌지 검사
        /// </summary>
        public static bool IsNotNull(object val)
        {
            return !IsNull(val);
        }
        /// <summary>
        /// DBNull 또는 null이 아닌지 검사
        /// </summary>
        public static bool IsNotNullOrEmpty(object val)
        {
            return !IsNullOrEmpty(val);
        }
        /// <summary>
        /// 문자열이 Null 또는 비었는지 검사
        /// </summary>
        public static bool IsStringNullOrEmpty(object val)
        {
            return (val == null || string.IsNullOrEmpty(val.ToString()));
        }
        /// <summary>
        /// 문자열이 Null 또는 비었는지 검사
        /// </summary>
        public static bool IsStringNullOrEmpty(string val)
        {
            return string.IsNullOrEmpty(val.ToString());
        }
        #endregion null, DBNull, EmptyString 체크

        #region TypeConvert ///////////////////////////////////////////////
        //참고 ADO.NET 
        // Generic version : public static T ReadValue<T>(object value)
        // Runtime Type version : public static object ReadValue(object value, Type targetType)

        /// <summary>
        /// Object별 기본값 반환 (값, 참조타입 지원)
        /// </summary>
        public static T DefaultValue<T>()
        {
            return (T)((typeof(T).IsValueType) ? Activator.CreateInstance(typeof(T)) : default(T));
        }

        /// <summary>
        /// Object -> CustomType 변환 (기본값 처리)
        /// </summary>
        public static T ChangeType<T>(object val) where T : IConvertible
        {
            T defaultValue = DefaultValue<T>();
            if (IsNotNullOrEmpty(val))
            {
                //T typedval = (T)val;  //cast error
                T typedval = (T)Convert.ChangeType(val, typeof(T));
                return (typedval != null) ? typedval : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Object -> CustomType 변환 (기본값 처리)
        /// </summary>
        public static T ChangeType<T>(object val, T defaultValue) where T : IConvertible
        {
            if (IsNotNullOrEmpty(val))
            {
                //T typedval = (T)val;  //cast error
                T typedval = (T)Convert.ChangeType(val, typeof(T));
                return (typedval != null) ? typedval : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Object -> CustomType 변환 (기본값 처리)
        /// </summary>
        public static T ChangeType<T>(object val, T defaultValue, IFormatProvider provider) where T : IConvertible
        {
            if (IsNotNullOrEmpty(val))
            {
                //T typedval = (T)val;  //cast error
                T typedval = (T)Convert.ChangeType(val, typeof(T), provider);
                return (typedval != null) ? typedval : defaultValue;
            }
            return defaultValue;
        }

        #region ToBoolean
        /// <summary>
        /// Object -> bool 변환 (기본값 처리)
        /// </summary>
        public static bool ToBoolean(object val, bool defaultValue)
        {
            return ChangeType<bool>(val, defaultValue);
        }
        /// <summary>
        /// Object -> bool 변환 (기본값 false - null인경우)
        /// </summary>
        public static bool ToBoolean(object val)
        {
            return ChangeType<bool>(val, false);
        }
        #endregion

        #region ToByte
        /// <summary>
        /// Object -> byte변환 (기본값 처리)
        /// </summary>
        public static byte ToByte(object val, byte defaultValue)
        {
            return ChangeType<byte>(val, defaultValue);
        }
        /// <summary>
        /// Object -> byte변환 (기본값 0 - null인경우)
        /// </summary>
        public static byte ToByte(object val)
        {
            return ChangeType<byte>(val, 0);
        }
        #endregion

        #region ToChar
        /// <summary>
        /// Object -> char변환 (기본값 처리)
        /// </summary>
        public static char ToChar(object val, char defaultValue)
        {
            return ChangeType<char>(val, defaultValue);
        }
        /// <summary>
        /// Object -> char변환 (기본값 \0 - null인경우)
        /// </summary>
        public static char ToChar(object val)
        {
            return ChangeType<char>(val, '\0');
        }
        #endregion

        #region ToDateTime
        /// <summary>
        /// Object -> DateTime 변환 (기본값 처리)
        /// </summary>
        public static DateTime ToDateTime(object val, DateTime defaultValue)
        {
            return ChangeType<DateTime>(val, defaultValue);
        }
        /// <summary>
        /// Object -> DateTime 변환 (기본값 DateTime.MinValue - null인경우)
        /// </summary>
        public static DateTime ToDateTime(object val)
        {
            return ChangeType<DateTime>(val, DateTime.MinValue);
        }
        #endregion

        #region ToDecimal
        /// <summary>
        /// Object -> decimal 변환 (기본값 처리)
        /// </summary>
        public static decimal ToDecimal(object val, decimal defaultValue)
        {
            return ChangeType<decimal>(val, defaultValue);
        }
        /// <summary>
        /// Object -> decimal 변환 (기본값 0M - null인경우)
        /// </summary>
        public static decimal ToDecimal(object val)
        {
            return ChangeType<decimal>(val, 0M);
        }
        #endregion

        #region ToDouble
        /// <summary>
        /// Object -> double변환 (기본값 처리)
        /// </summary>
        public static double ToDouble(object val, double defaultValue)
        {
            return ChangeType<double>(val, defaultValue);
        }
        /// <summary>
        /// Object -> double변환 (기본값 -1 - null인경우)
        /// </summary>
        public static double ToDouble(object val)
        {
            return ChangeType<double>(val, -1);
        }
        #endregion

        #region ToInt16, ToInt32, ToInt64, ToInt (32)
        /// <summary>
        /// Object -> Int16변환 (기본값 처리)
        /// </summary>
        public static Int16 ToInt16(object val, Int16 defaultValue)
        {
            return ChangeType<Int16>(val, defaultValue);
        }
        /// <summary>
        /// Object -> Int16변환 (기본값 -1 - null인경우)
        /// </summary>
        public static Int16 ToInt16(object val)
        {
            return ChangeType<Int16>(val, -1);
        }

        /// <summary>
        /// Object -> Int32변환 (기본값 처리)
        /// </summary>
        public static Int32 ToInt32(object val, Int32 defaultValue)
        {
            return ChangeType<Int32>(val, defaultValue);
        }
        /// <summary>
        /// Object -> Int32변환 (기본값 -1 - null인경우)
        /// </summary>
        public static Int32 ToInt32(object val)
        {
            return ChangeType<Int32>(val, -1);
        }

        /// <summary>
        /// Object -> Int64변환 (기본값 처리)
        /// </summary>
        public static Int64 ToInt64(object val, Int64 defaultValue)
        {
            return ChangeType<Int64>(val, defaultValue);
        }
        /// <summary>
        /// Object -> Int64변환 (기본값 -1 - null인경우)
        /// </summary>
        public static Int64 ToInt64(object val)
        {
            return ChangeType<Int64>(val, -1);
        }

        /// <summary>
        /// Object -> int변환 (기본값 처리)
        /// </summary>
        public static int ToInt(object val, int defaultValue)
        {
            return ChangeType<int>(val, defaultValue);
        }
        /// <summary>
        /// Object -> int변환 (기본값 -1 - null인경우)
        /// </summary>
        public static int ToInt(object val)
        {
            return ChangeType<int>(val, -1);
        }
        #endregion

        #region ToSByte
        /// <summary>
        /// Object -> sbyte변환 (기본값 처리)
        /// </summary>
        public static sbyte ToSByte(object val, sbyte defaultValue)
        {
            return ChangeType<sbyte>(val, defaultValue);
        }
        /// <summary>
        /// Object -> sbyte변환 (기본값 127 - null인경우)
        /// </summary>
        public static sbyte ToSByte(object val)
        {
            return ChangeType<sbyte>(val, 127);
        }
        #endregion

        #region ToSingle
        /// <summary>
        /// Object -> Single변환 (기본값 처리)
        /// </summary>
        public static Single ToSingle(object val, Single defaultValue)
        {
            return ChangeType<Single>(val, defaultValue);
        }
        /// <summary>
        /// Object -> Single변환 (기본값 Single.NaN - null인경우)
        /// </summary>
        public static Single ToSingle(object val)
        {
            return ChangeType<Single>(val, Single.NaN);
        }
        #endregion

        #region ToString
        /// <summary>
        /// Object -> String변환 (기본값, 포맷 지원)
        /// </summary>
        public static string ToString(object val, object defaultValue, string format)
        {
            return (IsNotNullOrEmpty(val)) ? string.Format(format, val) : string.Format(format, defaultValue);
        }
        /// <summary>
        /// Object -> String변환 (기본값 지원)
        /// </summary>
        public static string ToString(object val, object defaultValue)
        {
            return (IsNotNullOrEmpty(val)) ? val.ToString() : defaultValue.ToString();
        }
        /// <summary>
        /// Object -> String변환 (기본값 string.Empty)
        /// </summary>
        public static string ToString(object val)
        {
            return ToString(val, string.Empty);
        }
        #endregion

        #region ToUInt16, ToUInt32, ToUInt64, ToUInt (32)
        /// <summary>
        /// Object -> UInt16변환 (기본값 처리)
        /// </summary>
        public static UInt16 ToUInt16(object val, UInt16 defaultValue)
        {
            return ChangeType<UInt16>(val, defaultValue);
        }
        /// <summary>
        /// Object -> UInt16변환 (기본값 0 - null인경우)
        /// </summary>
        public static UInt16 ToUInt16(object val)
        {
            return ChangeType<UInt16>(val, 0);
        }

        /// <summary>
        /// Object -> UInt32변환 (기본값 처리)
        /// </summary>
        public static UInt32 ToUInt32(object val, UInt32 defaultValue)
        {
            return ChangeType<UInt32>(val, defaultValue);
        }
        /// <summary>
        /// Object -> UInt32변환 (기본값 0 - null인경우)
        /// </summary>
        public static UInt32 ToUInt32(object val)
        {
            return ChangeType<UInt32>(val, 0);
        }

        /// <summary>
        /// Object -> UInt64변환 (기본값 처리)
        /// </summary>
        public static UInt64 ToUInt64(object val, UInt64 defaultValue)
        {
            return ChangeType<UInt64>(val, defaultValue);
        }
        /// <summary>
        /// Object -> UInt64변환 (기본값 0 - null인경우)
        /// </summary>
        public static UInt64 ToUInt64(object val)
        {
            return ChangeType<UInt64>(val, 0);
        }

        /// <summary>
        /// Object -> uint변환 (기본값 처리)
        /// </summary>
        public static uint ToUInt(object val, uint defaultValue)
        {
            return ChangeType<uint>(val, defaultValue);
        }
        /// <summary>
        /// Object -> uint변환 (기본값 0 - null인경우)
        /// </summary>
        public static uint ToUInt(object val)
        {
            return ChangeType<uint>(val, 0);
        }
        #endregion

        #region ToCurrencyFormat : 숫자형Object -> 통화형 문자열
        /// <summary>
        /// Object -> 통화형 문자열
        /// </summary>
        public static string ToCurrencyFormat(object number)
        {
            return ToCurrencyFormat(number, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Object -> 통화형 문자열
        /// </summary>
        public static string ToCurrencyFormat(object number, string culture)
        {
            return string.Format("{0:N" + CurrencyDecimalDigits(culture) + "}", number);
        }

        /// <summary>
        /// Object -> 통화형 문자열
        /// </summary>
        public static string ToCurrencyFormat(object number, CultureInfo culture)
        {
            return string.Format("{0:N" + CurrencyDecimalDigits(culture) + "}", number);
        }

        #region 문화권 통화 소수자리수
        /// <summary>
        /// 현재 문화권에 따른 통화 값에 사용할 소수 자릿수
        /// </summary>
        private static int CurrencyDecimalDigits()
        {
            return CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits;
        }

        /// <summary>
        /// 문화권에 따른 통화 값에 사용할 소수 자릿수
        /// </summary>
        private static int CurrencyDecimalDigits(string culture)
        {
            return new CultureInfo(culture, false).NumberFormat.CurrencyDecimalDigits;
        }

        /// <summary>
        /// 문화권에 따른 통화 값에 사용할 소수 자릿수
        /// </summary>
        private static int CurrencyDecimalDigits(CultureInfo culture)
        {
            return culture.NumberFormat.CurrencyDecimalDigits;
        }
        #endregion
        #endregion

        #endregion TypeConvert

        #region Dictionary 헬퍼 (Dictionary->ListItem, Dictionary의 키와 값을 치환)

        /// <summary>
        /// Dictionary를 ListItem목록으로 변환한다.
        /// </summary>
        /// <typeparam name="T1">Dictionary Key Type</typeparam>
        /// <typeparam name="T2">Dictionary Value Type</typeparam>
        /// <param name="dic">dictionary</param>
        /// <returns>Dictionary에서 변환된 ListItem리스트</returns>
        //public static List<ListItem> ConvertToListItems<T1, T2>(Dictionary<T1, T2> dic)
        //{
        //    List<ListItem> items = new List<ListItem>();
        //    foreach (T1 key in dic.Keys)
        //    {
        //        items.Add(new ListItem(GMConvert.ToString(dic[key]), GMConvert.ToString(key)));
        //    }
        //    return items;
        //}

        /// <summary>
        /// Dictionary의 키와 값을 치환한다.
        /// </summary>
        /// <typeparam name="T1">Dictionary Key Type</typeparam>
        /// <typeparam name="T2">Dictionary Value Type</typeparam>
        /// <param name="dic">dictionary</param>
        /// <returns>키와 값이 치환된 사전</returns>
        public static Dictionary<T2, T1> SwitchKeyAndValueInDictionary<T1, T2>(Dictionary<T1, T2> dic)
        {
            Dictionary<T2, T1> revDic = new Dictionary<T2, T1>();
            foreach (T1 key in dic.Keys)
            {
                if (revDic.ContainsKey(dic[key]))
                {
                    throw new ArgumentException("사전의 값에 중복되는 값이 있어서, 키와 값을 치환할수 없습니다.", "dic");
                }
                revDic.Add(dic[key], key);
            }
            return revDic;
        }

        #endregion

        /// <summary>
        /// 파일사이즈를 문자열로 변환 (n -> nGB, nMB, nKB, nByte) 
        /// 1024단위로 나누어질때만 Unit을 상향조정
        /// </summary>
        /// <param name="fileLength">파일사이즈</param>
        /// <returns>파일 사이즈 문자열</returns>
        public static string ToFileSizeString(int fileLength)
        {
            if (fileLength % (1024 * 1024 * 1024) == 0)
                return (fileLength / (1024 * 1024 * 1024)).ToString() + "GB";
            if (fileLength % (1024 * 1024) == 0)
                return (fileLength / (1024 * 1024)).ToString() + "MB";
            if (fileLength % (1024) == 0)
                return (fileLength / (1024)).ToString() + "KB";
            return fileLength.ToString() + "Bytes";
        }

        /// <summary>
        /// 태그가 제거된 문자열로 변환합니다.
        /// </summary>
        /// <param name="contents">태그를 포함한 내용</param>
        /// <returns>태그가 제거된 문자열</returns>
        public static string ToTagRemovedString(string contents)
        {
            return Regex.Replace(contents, "<(/)?([a-zA-Z]*)(\\s[a-zA-Z]*=[^>]*)?(\\s)*(/)?>", "");
        }

        /// <summary>
        /// 태그가 제거되고 생략처리(...)된 문자열로 변환합니다.
        /// </summary>
        /// <param name="contents">태그를 포함한 내용</param>
        /// <param name="limitLength">전체 제한 길이</param>
        /// <param name="ellipsis">생략기호</param>
        /// <returns>태그가 제거되고 생략처리된 문자열</returns>
        public static string ToTagRemovedEllipsisString(string contents, int limitLength, string ellipsis)
        {
            if (string.IsNullOrEmpty(contents))
                return contents;

            contents = ToTagRemovedString(contents);

            if (contents.Length > limitLength)
            {
                //생략처리
                contents = contents.Substring(0, limitLength - ellipsis.Length) + ellipsis;
            }

            return contents;
        }

        /// <summary>
        /// 태그가 제거되고 생략처리(...)된 문자열로 변환합니다.
        /// </summary>
        /// <param name="contents">태그를 포함한 내용</param>
        /// <param name="limitLength">전체 제한 길이</param>
        /// <param name="ellipsis">생략기호</param>
        /// <returns>태그가 제거되고 생략처리된 문자열</returns>
        public static string ToTagRemovedEllipsisString(string contents, int limitLength)
        {
            return ToTagRemovedEllipsisString(contents, limitLength, "...");
        }

        /// <summary>
        /// null체크
        /// </summary>
        /// <param name="value">피대상자</param>
        /// <param name="returnVal">null인경우 대체값</param>
        /// <returns></returns>
        public static string IsNull(object value, string returnVal)
        {
            return (value == null ? returnVal : (string)value);
        }

        /// <summary>
        /// bool의 값을 1,0으로 대체
        /// </summary>
        /// <param name="val">bool value</param>
        /// <returns></returns>
        public static string Bool2String(bool val)
        {
            return (val ? "1" : "0");
        }

        /// <summary>
        /// 1,0의 값을 bool형으로 대체
        /// </summary>
        /// <param name="val">string value</param>
        /// <returns></returns>
        public static bool String2Bool(string val)
        {
            return (val == "1" ? true : false);
        }

        /// <summary>
        /// 2자리 문자로 맞춰주는 메소드
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static String SetTwoString(String val)
        {
            if (val.Length == 1)
                val = "0" + val;

            return val;
        }
        /// <summary>
        /// 앞자리를 가져오는 메소드
        /// 11, 13, 15, 18, 25, 32, 35
        /// 10, 10, 10, 10, 20, 30, 30
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static String GetTenOfTheMultiplier(String val)
        {
            if (val.Length == 1)
            {
                val = "0" + val;
            }
            else if (val.Length == 2)
            {
                val = val.Substring(0, 1) + "0";
            }
            else
            {
                val = "";
            }

            return val;
        }
    }
}
