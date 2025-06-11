using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public static class Config
    {
        #region [Postgre Database 설정관련]
        public static string dbIP
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["dbIP"]); }
        }

        public static string dbName
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["dbName"]); }
        }

        public static string dbPort
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["dbPort"]); }
        }

        public static string dbId
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["dbId"]); }
        }

        public static string dbPassword
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["dbPassword"]); }
        }
        #endregion

        #region [Oracle Database 설정관련]
        public static string OradbIP
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["OradbIP"]); }
        }

        public static string OradbName
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["OradbName"]); }
        }

        public static string OradbPort
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["OradbPort"]); }
        }

        public static string OradbId
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["OradbId"]); }
        }

        public static string OradbPassword
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["OradbPassword"]); }
        }
        #endregion

        #region [Sim Time]
        public static Int32 Rain_Real
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["Rain_Real"]); }
        }

        public static Int32 Report_Time
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["Report_Time"]); }
        }

        public static Int32 Rain_Forecast
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["Rain_Forecast"]); }
        }

        #endregion

        #region [SWMM 모의 관련]
        public static Int32 SWMMAuto_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["SWMMAuto_Caller_Second"]); }
        }

        public static Int32 SWMMAni_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["SWMMAni_Caller_Second"]); }
        }

        #endregion

        #region [NOMO 모의 관련]
        public static Int32 NOMOAuto_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["NOMOAuto_Caller_Second"]); }
        }
        #endregion

        #region [FloodMap 모의 관련]
        public static Int32 FloodMapAuto_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["FloodMapAuto_Caller_Second"]); }
        }
        #endregion

        #region [Wamis Dam자료 관련]
        public static Int32 OpenAPI_Wamis_mndtdata_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["OpenAPI_Wamis_mndtdata_Second"]); }
        }
        #endregion

        #region [KMA OpenAPI 관련]
        public static Int32 KMA_ASOS_Auto_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["KMA_ASOS_Auto_Caller_Second"]); }
        }
        #endregion

        #region [WAMIS Flow OpenAPI관련]

        public static Int32 WAMIS_Flow_Auto_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["WAMIS_Flow_Auto_Caller_Second"]); }
        }

        public static Int32 AG_tb_reserviorlevel_Auto_Caller_Second
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["AG_tb_reserviorlevel_Auto_Caller_Second"]); }
        }


        public static string RealTimeUse
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["RealTimeUse"]); }
        }

        public static string PeriodUse
        {
            get { return GMConvert.ToString(ConfigurationManager.AppSettings["PeriodUse"]); }
        }

        public static Int32 StartDate
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["StartDate"]); }
        }

        public static Int32 EndDate
        {
            get { return GMConvert.ToInt32(ConfigurationManager.AppSettings["EndDate"]); }
        }
        #endregion
        public static string DATA_ApiKey1 => ConfigurationManager.AppSettings["DATA_ApiKey1"];
        public static string DATA_ApiKey2 => ConfigurationManager.AppSettings["DATA_ApiKey2"];
        public static string DATA_ApiKey3 => ConfigurationManager.AppSettings["DATA_ApiKey3"];
        public static string authKeyASOS => ConfigurationManager.AppSettings["authKeyASOS"];

        public static string rScriptPath => ConfigurationManager.AppSettings["rScriptPath"];
        public static string rScriptPath2 => ConfigurationManager.AppSettings["rScriptPath2"];
        public static string rScriptPath3 => ConfigurationManager.AppSettings["rScriptPath3"];
        public static string rExePath => ConfigurationManager.AppSettings["rExePath"];
        
    }
}
