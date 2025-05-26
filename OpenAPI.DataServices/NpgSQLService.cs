using Npgsql;
using OpenAPI.Controls;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UFRI.FrameWork;
using static System.Net.Mime.MediaTypeNames;

namespace OpenAPI.DataServices
{
    public class NpgSQLService
    {
        private static string GetConnectionString()
        {
            string dbIP = Config.dbIP;
            string dbName = Config.dbName;
            string dbPort = Config.dbPort;
            string dbId = Config.dbId;
            string dbPassword = Config.dbPassword;

            string strConn = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                        dbIP, dbPort, dbId, dbPassword, dbName);

            return strConn;
        }

        #region [KMA]
        public static List<string> GetSggCD_FromOpenAPI_KMAASOS()
        {
            List<string> listSggCD = new List<string>();
            string strConn = GetConnectionString();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();

                    DataTable dt = new DataTable();
                    string query = string.Format("SELECT DISTINCT sgg_cd FROM drought.tb_kma_asos_thiessen order by sgg_cd");

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                    {
                        cmd.CommandText = query;
                        da.Fill(dt);
                    }

                    listSggCD = BizCommon.ConvertDataTableTostringList<string>(dt);

                    return listSggCD;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public static List<ASOSThiessen> GetThiessen_FromOpenAPI_KMAASOS()
        {
            List<ASOSThiessen> listThiessens = new List<ASOSThiessen>();

            string strConn = GetConnectionString();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();

                    DataTable dt = new DataTable();
                    string query = string.Format("SELECT * FROM drought.tb_kma_asos_thiessen");

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                    {
                        cmd.CommandText = query;
                        da.Fill(dt);
                    }

                    listThiessens = BizCommon.ConvertDataTableToList<ASOSThiessen>(dt);

                    return listThiessens;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }

        public static List<KMASiteInformation> GetSites_FromOpenAPI_KMAASOS()
        {
            List<KMASiteInformation> listSites = new List<KMASiteInformation>();

            string strConn = GetConnectionString();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();

                    DataTable dt = new DataTable();
                    string query = string.Format("SELECT * FROM drought.tb_kma_asos_sites");

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                    {
                        cmd.CommandText = query;
                        da.Fill(dt);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        KMASiteInformation addData = new KMASiteInformation();

                        addData.STD_ID = int.Parse(dt.Rows[i]["stn_id"].ToString());
                        addData.LON = double.Parse(dt.Rows[i]["lon"].ToString());
                        addData.LAT = double.Parse(dt.Rows[i]["lat"].ToString());
                        addData.STN_SP = dt.Rows[i]["stn_sp"].ToString();
                        addData.HT = double.Parse(dt.Rows[i]["ht"].ToString());
                        addData.HT_PA = double.Parse(dt.Rows[i]["ht_pa"].ToString());
                        addData.HT_TA = double.Parse(dt.Rows[i]["ht_ta"].ToString());
                        addData.HT_WD = double.Parse(dt.Rows[i]["ht_wd"].ToString());
                        addData.HT_RN = double.Parse(dt.Rows[i]["ht_rn"].ToString());
                        //addData.LAU_ID = int.Parse(dt.Rows[i]["lau_id"].ToString());
                        addData.STN_AD = int.Parse(dt.Rows[i]["stn_ad"].ToString());
                        addData.STN_KO = dt.Rows[i]["stn_ko"].ToString();
                        addData.STN_EN = dt.Rows[i]["stn_en"].ToString();
                        addData.FCT_ID = dt.Rows[i]["fct_id"].ToString();
                        addData.LAW_ID = dt.Rows[i]["law_id"].ToString();
                        //addData.BASIN = int.Parse(dt.Rows[i]["basin"].ToString());

                        listSites.Add(addData);
                    }

                    return listSites;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }

        public static bool BulkInsert_KMAASOSDatas(List<rcvKMAASOSData> lKMAASOSDatas)
        {
            string strConn = GetConnectionString();

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_kma_asos_dtdata(" +
                    "tm, stn, ws_avg, wr_day, wd_max, ws_max, ws_max_tm, wd_ins, ws_ins, ws_ins_tm, ta_avg, ta_max, ta_max_tm, ta_min, ta_min_tm, td_avg, ts_avg, tg_min, hm_avg, hm_min, hm_min_tm, pv_avg, ev_s, ev_l, fg_dur, pa_avg, ps_avg, ps_max, ps_max_tm, ps_min, ps_min_tm, ca_tot, ss_day, ss_dur, ss_cmb, si_day, si_60m_max, si_60m_max_tm, rn_day, rn_d99, rn_dur, rn_60m_max, rn_60m_max_tm, rn_10m_max, rn_10m_max_tm, rn_pow_max, rn_pow_max_tm, sd_new, sd_new_tm, sd_max, sd_max_tm, te_05, te_10, te_15, te_30, te_50) VALUES ");

                int i = 0;
                foreach (rcvKMAASOSData asos in lKMAASOSDatas)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53}, {54}, {55})"
                        , asos.TM, asos.STN, asos.WS_AVG, asos.WR_DAY, asos.WD_MAX, asos.WS_MAX, asos.WS_MAX_TM, asos.WD_INS, asos.WS_INS, asos.WS_INS_TM, asos.TA_AVG, asos.TA_MAX
                        , asos.TA_MAX_TM, asos.TA_MIN, asos.TA_MIN_TM, asos.TD_AVG, asos.TS_AVG, asos.TG_MIN, asos.HM_AVG, asos.HM_MIN, asos.HM_MIN_TM, asos.PV_AVG, asos.EV_S
                        , asos.EV_L, asos.FG_DUR, asos.PA_AVG, asos.PS_AVG, asos.PS_MAX, asos.PS_MAX_TM, asos.PS_MIN, asos.PS_MIN_TM, asos.CA_TOT, asos.SS_DAY, asos.SS_DUR
                        , asos.SS_CMB, asos.SI_DAY, asos.SI_60M_MAX, asos.SI_60M_MAX_TM, asos.RN_DAY, asos.RN_D99, asos.RN_DUR, asos.RN_60M_MAX, asos.RN_60M_MAX_TM, asos.RN_10M_MAX
                        , asos.RN_10M_MAX_TM, asos.RN_POW_MAX, asos.RN_POW_MAX_TM, asos.SD_NEW, asos.SD_NEW_TM, asos.SD_MAX, asos.SD_MAX_TM, asos.TE_05, asos.TE_10, asos.TE_15, asos.TE_30, asos.TE_50));

                    i++;
                }

                // ON CONFLICT 구문 추가 (tm, stn이 유니크 키라고 가정)
                query.Append(" ON CONFLICT (tm, stn) DO UPDATE SET " +
                    "ws_avg = EXCLUDED.ws_avg, wr_day = EXCLUDED.wr_day, wd_max = EXCLUDED.wd_max, ws_max = EXCLUDED.ws_max, ws_max_tm = EXCLUDED.ws_max_tm, " +
                    "wd_ins = EXCLUDED.wd_ins, ws_ins = EXCLUDED.ws_ins, ws_ins_tm = EXCLUDED.ws_ins_tm, ta_avg = EXCLUDED.ta_avg, ta_max = EXCLUDED.ta_max, " +
                    "ta_max_tm = EXCLUDED.ta_max_tm, ta_min = EXCLUDED.ta_min, ta_min_tm = EXCLUDED.ta_min_tm, td_avg = EXCLUDED.td_avg, ts_avg = EXCLUDED.ts_avg, " +
                    "tg_min = EXCLUDED.tg_min, hm_avg = EXCLUDED.hm_avg, hm_min = EXCLUDED.hm_min, hm_min_tm = EXCLUDED.hm_min_tm, pv_avg = EXCLUDED.pv_avg, " +
                    "ev_s = EXCLUDED.ev_s, ev_l = EXCLUDED.ev_l, fg_dur = EXCLUDED.fg_dur, pa_avg = EXCLUDED.pa_avg, ps_avg = EXCLUDED.ps_avg, ps_max = EXCLUDED.ps_max, " +
                    "ps_max_tm = EXCLUDED.ps_max_tm, ps_min = EXCLUDED.ps_min, ps_min_tm = EXCLUDED.ps_min_tm, ca_tot = EXCLUDED.ca_tot, ss_day = EXCLUDED.ss_day, " +
                    "ss_dur = EXCLUDED.ss_dur, ss_cmb = EXCLUDED.ss_cmb, si_day = EXCLUDED.si_day, si_60m_max = EXCLUDED.si_60m_max, si_60m_max_tm = EXCLUDED.si_60m_max_tm, " +
                    "rn_day = EXCLUDED.rn_day, rn_d99 = EXCLUDED.rn_d99, rn_dur = EXCLUDED.rn_dur, rn_60m_max = EXCLUDED.rn_60m_max, rn_60m_max_tm = EXCLUDED.rn_60m_max_tm, " +
                    "rn_10m_max = EXCLUDED.rn_10m_max, rn_10m_max_tm = EXCLUDED.rn_10m_max_tm, rn_pow_max = EXCLUDED.rn_pow_max, rn_pow_max_tm = EXCLUDED.rn_pow_max_tm, " +
                    "sd_new = EXCLUDED.sd_new, sd_new_tm = EXCLUDED.sd_new_tm, sd_max = EXCLUDED.sd_max, sd_max_tm = EXCLUDED.sd_max_tm, te_05 = EXCLUDED.te_05, " +
                    "te_10 = EXCLUDED.te_10, te_15 = EXCLUDED.te_15, te_30 = EXCLUDED.te_30, te_50 = EXCLUDED.te_50");

                using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
                {
                    conn.Open();
                    var command = new NpgsqlCommand(query.ToString(), conn);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            /*
                     public static bool BulkInsert_KMAASOSDatas(List<rcvKMAASOSData> lKMAASOSDatas)
                    {
                        string strConn = GetConnectionString();

                        try
                        {
                            StringBuilder query = new StringBuilder();
                            query.Append("INSERT INTO drought.tb_kma_asos_dtdata(tm, stn, ws_avg, wr_day, wd_max, ws_max, ws_max_tm, wd_ins, ws_ins, ws_ins_tm, ta_avg, ta_max, ta_max_tm, ta_min, ta_min_tm, td_avg, ts_avg, tg_min, hm_avg, hm_min, hm_min_tm, pv_avg, ev_s, ev_l, fg_dur, pa_avg, ps_avg, ps_max, ps_max_tm, ps_min, ps_min_tm, ca_tot, ss_day, ss_dur, ss_cmb, si_day, si_60m_max, si_60m_max_tm, rn_day, rn_d99, rn_dur, rn_60m_max, rn_60m_max_tm, rn_10m_max, rn_10m_max_tm, rn_pow_max, rn_pow_max_tm, sd_new, sd_new_tm, sd_max, sd_max_tm, te_05, te_10, te_15, te_30, te_50) VALUES ");

                            int i = 0;
                            foreach (rcvKMAASOSData asos in lKMAASOSDatas)
                            {
                                if (i != 0)
                                {
                                    query.Append(" , ");
                                }

                                query.Append(string.Format("('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53}, {54}, {55})"
                                    , asos.TM, asos.STN, asos.WS_AVG, asos.WR_DAY, asos.WD_MAX, asos.WS_MAX, asos.WS_MAX_TM, asos.WD_INS, asos.WS_INS, asos.WS_INS_TM, asos.TA_AVG, asos.TA_MAX
                                    , asos.TA_MAX_TM, asos.TA_MIN, asos.TA_MIN_TM, asos.TD_AVG, asos.TS_AVG, asos.TG_MIN, asos.HM_AVG, asos.HM_MIN, asos.HM_MIN_TM, asos.PV_AVG, asos.EV_S
                                    , asos.EV_L, asos.FG_DUR, asos.PA_AVG, asos.PS_AVG, asos.PS_MAX, asos.PS_MAX_TM, asos.PS_MIN, asos.PS_MIN_TM, asos.CA_TOT, asos.SS_DAY, asos.SS_DUR
                                    , asos.SS_CMB, asos.SI_DAY, asos.SI_60M_MAX, asos.SI_60M_MAX_TM, asos.RN_DAY, asos.RN_D99, asos.RN_DUR, asos.RN_60M_MAX, asos.RN_60M_MAX_TM, asos.RN_10M_MAX
                                    , asos.RN_10M_MAX_TM, asos.RN_POW_MAX, asos.RN_POW_MAX_TM, asos.SD_NEW, asos.SD_NEW_TM, asos.SD_MAX, asos.SD_MAX_TM, asos.TE_05, asos.TE_10, asos.TE_15, asos.TE_30, asos.TE_50));

                                i++;
                            }

                            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
                            {
                                conn.Open();
                                var command = new NpgsqlCommand(query.ToString(), conn);
                                command.ExecuteNonQuery();

                                return true;
                            }
                        }


             */

            catch (Exception ex)
            {
                GMLogHelper.WriteLog(ex.StackTrace);
                GMLogHelper.WriteLog(ex.Message);
                GMLogHelper.WriteLog($"[ERROR] [ASOS] [Database] BulkInsert_KMAASOSDatas 예외: {ex.Message}");
                if (ex.InnerException != null)
                    GMLogHelper.WriteLog($"[ERROR] [ASOS] [Database] InnerException: {ex.InnerException.Message}");
                return false;
            }
        }

        public static PointRainfall GetDailyRainfall_FromOpenAPI_KMAASOS(int stnID, DateTime sDate, DateTime eDate)
        {
            string strConn = GetConnectionString();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    PointRainfall pData = new PointRainfall();
                    pData.stn = stnID.ToString();

                    conn.Open();

                    string query = string.Format("SELECT * FROM drought.tb_kma_asos_dtdata WHERE stn = {0} AND tm >= '{1}' AND tm <= '{2}' ORDER BY tm", stnID, sDate.ToString("yyyy-MM-dd HH:mm:ss"), eDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    DataTable dt = DirectQuery(query, conn);

                    List<tsTimeSeries> listRainfall = new List<tsTimeSeries>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        tsTimeSeries rn = new tsTimeSeries();

                        rn.tm = DT_GetTostring(dr, "tm");
                        rn.tmdt = BizCommon.StringtoDateTime(rn.tm, "yyyyMMdd");
                        //rn.DayOfYear = rn.tmdt.DayOfYear;

                        rn.rainfall = DT_GetTodouble(dr, "rn_day");

                        //CookieMon
                        //윤달제거
                        if (rn.tmdt.Month == 2 && rn.tmdt.Day == 29)
                        {

                        }
                        else
                        {
                            listRainfall.Add(rn);
                        }                        
                    }

                    pData.listRainfall = listRainfall;

                    return pData;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }

        public static List<rcvKMAASOSData> GetDailyDatas_FromOpenAPI_KMAASOS(int stnID, DateTime sDate, DateTime eDate)
        {
            string strConn = GetConnectionString();
            List<rcvKMAASOSData> lDBDatas = new List<rcvKMAASOSData>();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();

                    string query = string.Format("SELECT * FROM drought.tb_kma_asos_dtdata WHERE stn = {0} AND tm >= '{1}' AND tm <= '{2}' ORDER BY tm", stnID, sDate.ToString("yyyy-MM-dd HH:mm:ss"), eDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    DataTable dt = DirectQuery(query, conn);

                    foreach (DataRow dr in dt.Rows)
                    {
                        rcvKMAASOSData addData = new rcvKMAASOSData();

                        addData.STN = DT_GetToInt(dr, "stn");
                        addData.TM = DT_GetTostring(dr, "tm");

                        //CookieMon

                        lDBDatas.Add(addData);
                    }

                    return lDBDatas;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }

        #endregion

        #region [WAMIS]

    //    WITH RECURSIVE boundary AS(
    //SELECT
    //    MIN(obsdh)::TEXT AS start_ymd,
    //    MAX(obsdh)::TEXT AS end_ymd
    //FROM drought.tb_wamis_mnhrdata
    //WHERE damcd = '4105210' AND obsdh BETWEEN '2024123001' AND '2024123117'
    //    ),
    //    date_series AS(
    //        SELECT TO_TIMESTAMP(start_ymd, 'YYYYMMDDHH24') AS ymd, start_ymd, end_ymd
    //        FROM boundary
    //        UNION ALL
    //        SELECT ds.ymd + INTERVAL '1 hour', ds.start_ymd, ds.end_ymd
    //        FROM date_series ds
    //        WHERE ds.ymd<TO_TIMESTAMP(SUBSTRING(ds.end_ymd, 1, 8) || '17', 'YYYYMMDDHH24') -- 수정된 부분
    //    )
    //    SELECT DISTINCT to_char(ds.ymd, 'YYYYMMDDHH') AS ymd, m.*  -- DISTINCT 추가
    //    FROM date_series ds
    //    LEFT JOIN drought.tb_wamis_mnhrdata m
    //        ON to_char(ds.ymd, 'YYYYMMDDHH') = m.obsdh
    //        AND m.damcd = '4105210'
    //    WHERE m.obsdh BETWEEN '2024123001' AND '2024123117'
    //    ORDER BY m.obsdh;

        public static List<DamHRData> GetDailyDatasFromOpenAPIWAMISDamHrData_TimeCorrection(string damcd)
        {
            string strConn = GetConnectionString();
            List<DamHRData> lDBDatas = new List<DamHRData>();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    string query = string.Format("SELECT * FROM drought.tb_wamis_mnhrdata WHERE damcd = '{0}' ORDER BY obsdh", damcd);
                    DataTable dt = DirectQuery(query, conn);

                    lDBDatas = BizCommon.ConvertDataTableToList<DamHRData>(dt);

                    return lDBDatas;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }


        public static List<DamHRData> GetDailyDatasFromOpenAPIWAMISDamHrData(string damcd, string sDate, string eDate)
        {
            string strConn = GetConnectionString();
            List<DamHRData> lDBDatas = new List<DamHRData>();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    string query = string.Format("SELECT * FROM drought.tb_wamis_mnhrdata WHERE damcd = '{0}' AND obsdh >= '{1}' AND obsdh <= '{2}' ORDER BY obsdh", damcd, sDate, eDate);
                    DataTable dt = DirectQuery(query, conn);

                    lDBDatas = BizCommon.ConvertDataTableToList<DamHRData>(dt);

                    return lDBDatas;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }
        public static DateTime GetLastDateFromOpenAPI_WAMIS_mnhrdata()
        {
            string strConn = GetConnectionString();
            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(obsdh) FROM drought.tb_wamis_mnhrdata";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            DateTime dateTime;
                            // obsdh 형식은 'YYYYMMDDHH'로 가정 (예: 2024123117)
                            if (DateTime.TryParseExact(result.ToString(), "yyyyMMddHH", null, System.Globalization.DateTimeStyles.None, out dateTime))
                            {
                                return dateTime;
                            }
                            else
                            {
                                // 변환 실패 시 기본값 반환
                                return DateTime.Now.AddDays(-30);
                            }
                        }
                        else
                        {
                            // result가 DBNull.Value인 경우에 대한 처리
                            return DateTime.Now.AddDays(-30);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(ex.StackTrace);
                    GMLogHelper.WriteLog(ex.Message);
                    return DateTime.MinValue;
                }
            }
        }





        public static List<FlowData> GetDailyDatasFromOpenAPIWAMISFlow(string obsCD, string sDate, string eDate)
        {
            string strConn = GetConnectionString();
            List<FlowData> lDBDatas = new List<FlowData>();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    string query = string.Format("SELECT * FROM drought.tb_wamis_flowdtdata WHERE obscd = '{0}' AND ymd >= '{1}' AND ymd <= '{2}' ORDER BY ymd", obsCD, sDate, eDate);
                    DataTable dt = DirectQuery(query, conn);

                    lDBDatas = BizCommon.ConvertDataTableToList<FlowData>(dt);

                    return lDBDatas;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }

        public static bool BulkInsert_WAMISDamHrDatas(List<DamHRData> damHrDatas)
        {
            string strConn = GetConnectionString();

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_wamis_mnhrdata (damcd, obsdh, rwl, ospilwl, rsqty, rsrt, iqty, etqty, tdqty, edqty, spdqty, otltdqty, itqty, dambsarf) VALUES ");

                int i = 0;
                foreach (DamHRData data in damHrDatas)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')", data.damcd, data.obsdh, data.rwl, data.ospilwl, data.rsqty, data.rsrt
                        , data.iqty, data.etqty, data.tdqty, data.edqty, data.spdqty, data.otltdqty, data.itqty, data.dambsarf));

                    i++;
                }

                using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
                {
                    conn.Open();
                    var command = new NpgsqlCommand(query.ToString(), conn);
                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(ex.StackTrace);
                GMLogHelper.WriteLog(ex.Message);

                return false;
            }
        }
        public static DateTime GetLastDateFromOpenAPI_KMAASOS()
        {
            string strConn = GetConnectionString();
            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(tm) FROM drought.tb_kma_asos_dtdata";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            DateTime dateTime;
                            if (DateTime.TryParseExact(result.ToString(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dateTime))
                            {
                                return dateTime;
                            }
                            else
                            {
                                // 변환 실패 시 기본값 반환
                                return DateTime.Now.AddDays(-30);
                            }
                        }
                        else
                        {
                            // result가 DBNull.Value인 경우에 대한 처리 추가
                            return DateTime.Now.AddDays(-30);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(ex.StackTrace);
                    GMLogHelper.WriteLog(ex.Message);
                    return DateTime.MinValue;
                }
            }
        }
        /*백업1
        public static bool BulkInsert_WAMISFlowDatas(List<FlowData> flowDatas)
        {
            string strConn = GetConnectionString();

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_wamis_flowdtdata(obscd, ymd, flow) VALUES ");

                int i = 0;
                foreach (FlowData data in flowDatas)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    double? flowValue = null;

                    if (double.IsNaN(data.flw))
                    {
                        flowValue = -9999;
                    }
                    else
                    {
                        flowValue = data.flw;
                    }

                    query.Append(string.Format("('{0}', '{1}', {2})", data.obscd, data.ymd, flowValue));

                    i++;
                }

                using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
                {
                    conn.Open();
                    var command = new NpgsqlCommand(query.ToString(), conn);
                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(ex.StackTrace);
                GMLogHelper.WriteLog(ex.Message);

                return false;
            }
        }*/
        public static bool BulkInsert_WAMISFlowDatas(List<FlowData> flowDatas)
        {
            string strConn = GetConnectionString();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_wamis_flowdtdata(obscd, ymd, flow) VALUES ");
                int i = 0;
                foreach (FlowData data in flowDatas)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    double? flowValue = null;
                    if (double.IsNaN(data.flw))
                    {
                        flowValue = -9999;
                    }
                    else
                    {
                        flowValue = data.flw;
                    }

                    query.Append(string.Format("('{0}', '{1}', {2})", data.obscd, data.ymd, flowValue));
                    i++;
                }

                // UPSERT 구문 추가
                query.Append(" ON CONFLICT (obscd, ymd) DO UPDATE SET " +
                             "flow = CASE " +
                             "WHEN drought.tb_wamis_flowdtdata.flow = -9999 AND EXCLUDED.flow != -9999 THEN EXCLUDED.flow " +
                             "ELSE drought.tb_wamis_flowdtdata.flow " +
                             "END");

                using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
                {
                    conn.Open();
                    var command = new NpgsqlCommand(query.ToString(), conn);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(ex.StackTrace);
                GMLogHelper.WriteLog(ex.Message);
                return false;
            }
        }
        public static List<AgriDamSpec> Get_AgriDamSpec()
        {
            List<AgriDamSpec> listAgriDam = new List<AgriDamSpec>();

            string strConn = GetConnectionString();

            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();

                    DataTable dt = new DataTable();
                    string query = string.Format("SELECT * FROM drought.tb_reservior");

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                    {
                        cmd.CommandText = query;
                        da.Fill(dt);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        AgriDamSpec addData = new AgriDamSpec();

                        addData.facCode = dt.Rows[i]["fac_code"].ToString();
                        addData.facName = dt.Rows[i]["fac_name"].ToString();
                        addData.facAdd = dt.Rows[i]["county"].ToString();

                        listAgriDam.Add(addData);
                    }

                    return listAgriDam;
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                    GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                    return null;
                }
            }
        }

        public static DateTime GetLastDateFromOpenAPI_WAMIS_Flow()
        {
            string strConn = GetConnectionString();
            using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(TO_DATE(ymd, 'YYYYMMDD')) FROM drought.tb_wamis_flowdtdata";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            return Convert.ToDateTime(result);
                        }
                        else
                        {
                            // 데이터가 없는 경우 기본값 반환
                            return DateTime.Now.AddDays(-30);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(ex.StackTrace);
                    GMLogHelper.WriteLog(ex.Message);
                    return DateTime.MinValue;
                }
            }
        }
        public static bool BulkInsert_ReservoirLevelData(List<ReservoirLevelData> dataList)
        {
            string strConn = GetConnectionString();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_reserviorlevel (fac_code, check_date, county, fac_name, rate) VALUES ");

                int i = 0;
                foreach (ReservoirLevelData data in dataList)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }



                    query.Append(string.Format("('{0}', '{1}', '{2}', '{3}', '{4}')",
                        data.fac_code, data.check_date, data.county, data.fac_name, data.rate));
                    i++;
                }

                // 중복 데이터 처리를 위한 UPSERT 구문 추가
                query.Append(" ON CONFLICT (fac_code, check_date) DO UPDATE SET " +
                            "county = EXCLUDED.county, " +
                            "fac_name = EXCLUDED.fac_name, " +
                            "rate = EXCLUDED.rate " );

                using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
                {
                    conn.Open();
                    var command = new NpgsqlCommand(query.ToString(), conn);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(ex.StackTrace);
                GMLogHelper.WriteLog(ex.Message);
                return false;
            }
        }
        public static DateTime GetLastDateFromOpenAPI_AG_tb_reserviorlevel()
        {
            try
            {
                string sql = "SELECT MAX(check_date) FROM drought.tb_reserviorlevel";
                using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            // 날짜 형식에 맞게 파싱 (예: "20250408" -> DateTime)
                            string dateStr = result.ToString();
                            Console.WriteLine($"데이터베이스에서 반환된 날짜 문자열: '{dateStr}'");
                            bool parseSuccess = DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date);
                            Console.WriteLine($"날짜 파싱 성공 여부: {parseSuccess}");

                            if (parseSuccess)
                            {
                                return date;
                            }
                        }
                    }
                }
                // 기본값으로 30일 전 날짜 반환
                return DateTime.Today.AddDays(-30);
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"GetLastDateFromOpenAPI_AG_tb_reserviorlevel 오류: {ex.Message}");
                // 오류 발생 시 기본값으로 30일 전 날짜 반환
                return DateTime.Today.AddDays(-30);
            }
        }
        public static List<ReservoirLevelData> GetReservoirLevelData(string facCode, string startDate, string endDate)
        {
            List<ReservoirLevelData> result = new List<ReservoirLevelData>();
            try
            {
                string query = @"SELECT check_date, county, fac_code, fac_name, rate 
                        FROM drought.tb_reserviorlevel 
                        WHERE fac_code = @facCode 
                        AND check_date BETWEEN @startDate AND @endDate";

                using (NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@facCode", facCode);
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReservoirLevelData data = new ReservoirLevelData
                                {
                                    check_date = reader["check_date"].ToString(),
                                    county = reader["county"].ToString(),
                                    fac_code = reader["fac_code"].ToString(),
                                    fac_name = reader["fac_name"].ToString(),
                                    rate = reader["rate"].ToString()
                                };
                                result.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"GetReservoirLevelData 오류: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
            }
            return result;
        }

        #endregion

        #region [Database 처리]
        public static int StringNonQuery(string sQuery, NpgsqlConnection npgSQLconn)
        {
            return DirectNonQuery(sQuery, npgSQLconn);
        }

        public static int DirectNonQuery(string query, NpgsqlConnection npgSQLconn)
        {
            NpgsqlCommand sc = GetSqlQueryCommand(query, npgSQLconn);
            int iResult = ExecuteNonQuery(sc);
            return iResult;
        }

        public static int ExecuteNonQuery(NpgsqlCommand command)
        {
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 직접 쿼리결과를 가져오는 함수
        /// </summary>
        /// <param name="sQuery"></param>
        /// <returns></returns>
        public static DataTable DirectQuery(string query, NpgsqlConnection npgSQLconn)
        {
            NpgsqlCommand sc = GetSqlQueryCommand(query, npgSQLconn);
            return LoadDataTable(sc, string.Empty);
        }

        public static NpgsqlCommand GetSqlQueryCommand(string query, NpgsqlConnection npgSQLconn)
        {
            return PrepareCommand(CommandType.Text, query, npgSQLconn);
        }

        private static NpgsqlCommand PrepareCommand(CommandType commandType, string commandText, NpgsqlConnection npgSQLconn)
        {
            NpgsqlCommand command = new NpgsqlCommand(commandText, npgSQLconn);
            command.CommandType = commandType;
            return command;
        }

        public static DataTable LoadDataTable(NpgsqlCommand command, string tableName)
        {
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(command))
            {
                using (DataTable dt = new DataTable(tableName))
                {
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        #endregion

        #region [Data Convert]
        private static int DT_GetToInt(DataRow row, string sColumnName)
        {
            return toInteger(row, sColumnName, 0);
        }

        public static int toInteger(DataRow row, string sColumnName, int nDefaultValue)
        {
            int nResult = nDefaultValue;
            if (row.Table.Columns.Contains(sColumnName))
            {
                object value = row[sColumnName];
                if ((value is DBNull) == false && value != null)
                {
                    string sValue = value.ToString();
                    if (int.TryParse(sValue, out nResult) == false)
                        nResult = nDefaultValue;
                }
            }
            return nResult;
        }

        private static string DT_GetTostring(DataRow row, string sColumnName)
        {
            return toString(row, sColumnName, string.Empty);
        }

        public static string toString(DataRow row, string sColumnName, string sDefaultValue)
        {
            string sResult = sDefaultValue;
            if (row.Table.Columns.Contains(sColumnName))
            {
                object value = row[sColumnName];
                if ((value is DBNull) == false && value != null)
                {
                    sResult = value.ToString();
                }
            }
            sResult = sResult.Trim();
            return sResult;
        }

        public static double DT_GetTodouble(DataRow row, string sColumnName)
        {
            return toDouble(row, sColumnName, 0);
        }

        public static double toDouble(DataRow row, string sColumnName, double dDefaultValue)
        {
            double dResult = dDefaultValue;
            if (row.Table.Columns.Contains(sColumnName))
            {
                object value = row[sColumnName];
                if ((value is DBNull) == false && value != null)
                {
                    string sValue = value.ToString();
                    if (double.TryParse(sValue, out dResult) == false)
                        dResult = dDefaultValue;
                }
            }
            return dResult;
        }

        public static DateTime DT_GetToDateTime(DataRow row, string sColumnName)
        {
            return toDateTime(row, sColumnName, DateTime.MinValue);
        }

        public static DateTime toDateTime(DataRow row, string sColumnName, DateTime defaultTime)
        {
            DateTime result = defaultTime;

            if (row.Table.Columns.Contains(sColumnName))
            {
                object value = row[sColumnName];
                if ((value is DBNull) == false && value != null)
                {
                    try
                    {
                        result = (DateTime)value;
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }
            }
            return result;
        }

        











        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////


    }
}
