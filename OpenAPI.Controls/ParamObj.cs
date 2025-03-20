using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Controls
{
    public class ParamObj
    {
        public DataTable obscdDT { get; }        // 관측소 코드 목록
        public string[] colArr { get; }          // 데이터별 테이블 칼럼
        public string apiAddr { get; }           // 호출할 api 주소값
        public string saveFileName { get; }      // 저장할 엑셀 파일 명
        public string[] weSaveFileName { get; } // 기상 일자료 저장할 엑셀 파일명

        WamisAPIService apiService = new WamisAPIService();


        public ParamObj(string dataType)
        {
            if (dataType == "wl_obs") //수위관측소 제원
            {
                obscdDT = apiService.getList("wl_dubwlobs", "");
                colArr = new string[] { "obsnm", "wlobscd", "mggvcd", "bbsncd", "sbsncd", "obsopndt", "obskdcd", "rivnm", "bsnara"
                                        , "rvwdt", "bedslp", "rvmjctdis", "wsrdis", "addr", "lon", "lat", "tmx", "tmy", "gdt"
                                        , "wltel", "tdeyn", "mxgrd", "sistartobsdh", "siendobsdh", "olstartobsdh", "olendobsdh" };
                apiAddr = "wl_obsinfo";
                saveFileName = "wl_obsinfo";
            }
            else if (dataType == "fl_obs") //유량관측소 제원
            {
                obscdDT = apiService.getList("flw_dubobsif", "");
                //colArr = new string[] { "obsnm", "wlobscd", "mggvcd", "bbsncd", "sbsncd", "obsopndt", "obskdcd", "rivnm", "bsnara"
                //                        , "rvwdt", "bedslp", "rvmjctdis", "wsrdis", "addr", "lon", "lat", "tmx", "tmy", "gdt"
                //                        , "wltel", "tdeyn", "mxgrd", "sistartobsdh", "siendobsdh", "olstartobsdh", "olendobsdh" };
                //apiAddr = "wl_obsinfo";
                //saveFileName = "wl_obsinfo";
                colArr = new string[] { "bbsnnm", "obscd", "obsnm", "minyear", "maxyear", "sbsncd", "mngorg" };
                apiAddr = "flw_dubobsif";
                saveFileName = "flw_dubobsif";
            }
            else if (dataType == "rf_obs") //강우관측소 제원
            {
                obscdDT = apiService.getList("rf_dubrfobs", "");
                colArr = new string[] { "obsnm", "obscd", "mngorg", "bbsnnm", "sbsncd", "opendt", "obsknd", "addr", "lon"
                                        , "lat", "shgt", "hrdtstart", "hrdtend", "dydtstart", "dydtend" };
                apiAddr = "rf_obsinfo";
                saveFileName = "rf_obsinfo";
            }
            else if (dataType == "we_obs")  // 기상 관측소 제원
            {
                obscdDT = apiService.getList("we_dwtwtobs", "");
                colArr = new string[] { "wtobsocd", "wtobscd", "obsnm", "sbsncd", "clsyn", "obskdcd", "mggvcd", "opndt",
                                        "lat", "lon", "tmx", "tmy", "addr", "bbsncd", "obselm", "thrmlhi", "prselm", "wvmlhi",
                                        "hytmlhi", "orgcha", "nj", "wthsp", "spitm", "avgtmrPD", "mxtmrpd", "mntmrpd", "mnsnwpd",
                                        "smlevap", "lagevap", "avgwvpd", "mxdwdpd", "mxwvpd", "avghmdpd", "mnhmdpd", "avgdpnt",
                                        "avgcdqty", "avgsprs", "hrzsunpd", "snsnpd" };
                apiAddr = "we_obsinfo";
                saveFileName = "weather_obsinfo";
            }
            else if (dataType == "wl_data")
            {
                obscdDT = apiService.getList("wl_dtdata", "");
                colArr = new string[] { "ymd", "wl" };
                apiAddr = "wl_dtdata";
                saveFileName = "wl_DailyData";
            }
            else if (dataType == "flow_data")
            {
                obscdDT = apiService.getList("flw_dubobsif", "");
                colArr = new string[] { "ymd", "fw" };
                apiAddr = "flw_dtdata";
                saveFileName = "flw_DailyData";
            }
            else if (dataType == "rf_data")  // 강수 일자료
            {
                obscdDT = apiService.getList("rf_dubrfobs", "");
                colArr = new string[] { "ymd", "rf" };
                apiAddr = "rf_dtdata";
                saveFileName = "rf_DailyData";
            }
            else if (dataType == "we_data")  // 기상 일자료   // 배열로 저장 항목 받아서 저장해야 할듯
            {
                obscdDT = apiService.getList("we_dwtwtobs", "");
                colArr = new string[] { "ymd", "taavg", "tamin", "tamax", "wsavg", "hmavg", "evs", "evl", "siavg", "ssavg" };
                apiAddr = "we_dtdata";
                weSaveFileName = new string[] { "weather_DailyData_taavg", "weather_DailyData_tamin", "weather_DailyData_tamax",
                                                "weather_DailyData_wsavg", "weather_DailyData_hmavg", "weather_DailyData_evs",
                                                "weather_DailyData_evl", "weather_DailyData_siavg", "weather_DailyData_ssavg" };

            }
        }
    }
}
