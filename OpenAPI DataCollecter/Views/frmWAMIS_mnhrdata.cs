using DevExpress.Xpo.DB.Helpers;
using Npgsql;
using OpenAPI.Controls;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace OpenAPI_DataCollecter
{
    public partial class frmWAMIS_mnhrdata : Form
    {
        public Global _global { get; set; }

        #region [Delegate]
        delegate void WriteToStatusCallback(string message);    //스레드교착
        #endregion

        public frmWAMIS_mnhrdata()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmWAMIS_mnhrdata_Load(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.dtpStart.Value = new DateTime(1992, 01, 01);
            this.dtpEnd.Value = new DateTime(2024, 12, 31);
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnSearch":
                    Search_DamhourlyData();
                    break;
                default:
                    break;
            }
        }

        private void Search_DamhourlyData()
        {
            WamisAPIService apiService = new WamisAPIService();
            WamisParamObj paramObj = new WamisParamObj("mn_hrdata"); // 파라미터 객체
            DataTable rtnTable = new DataTable();         // 저장할 테이블

            try
            {
                DateTime stDate = this.dtpStart.Value;
                DateTime edDate = this.dtpEnd.Value;

                for (DateTime dt = stDate; dt <= edDate; dt = dt.AddMonths(6))
                {
                    //DateTime Search_stDate = new DateTime(dt.Year, dt.Month, 1, );
                    //int lastday = DateTime.DaysInMonth(dt.Year, dt.Month + 5);
                    //DateTime Search_edDate = new DateTime(dt.Year, dt.Month + 5, lastday);

                    //string message = string.Format("Start Date : {0} - End Date : {1}", Search_stDate.ToString("f"), Search_edDate.ToString("f"));
                    //WriteToStatus(message);

                    DateTime Search_stDate = new DateTime(dt.Year, dt.Month, 1, 1, 0, 0);
                    int lastday = DateTime.DaysInMonth(dt.Year, dt.Month + 5);
                    DateTime Search_edDate = new DateTime(dt.Year, dt.Month + 5, lastday, 23, 59, 0);

                    foreach (DataRow dr in paramObj.dtDamCD.Rows)
                    {
                        string damCD = dr["damcd"].ToString().Trim();

                        DataTable damData = new DataTable();
                        damData = apiService.getList(paramObj.apiAddr, damCD, Search_stDate.ToString("yyyyMMdd"), Search_edDate.ToString("yyyyMMdd"));

                        if (damData != null)
                        {
                            string message = string.Format("GetData => damCD = {0}, RequestYear = {1}, Data Count = {2}", damCD, Search_stDate.Year, damData.Rows.Count);
                            WriteToStatus(message);

                            //DataInsert

                            if (DataInsert3(damCD, damData) == true)
                            {
                                message = string.Format("DataInsert OK => damCD = {0}, RequestYear = {1}, Data Count = {2}", damCD, Search_stDate.Year, damData.Rows.Count);
                                WriteToStatus(message);
                            }
                            else
                            {
                                message = string.Format("DataInsert Fail => damCD = {0}, RequestYear = {1}, Data Count = {2}", damCD, Search_stDate.Year, damData.Rows.Count);
                                WriteToStatus(message);
                            }
                        }
                        else
                        {
                            string message = string.Format("damCD = {0}, RequestYear = {1}, Data Count = 0", damCD, Search_stDate.Year);
                            WriteToStatus(message);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                throw;
            }
        }

        private bool DataInsert3(string damCD, DataTable damData)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                //var transaction = _global.NpgSQLconn.BeginTransaction();
                                
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_wamis_mnhrdata (damcd, obsdh, rwl, ospilwl, rsqty, rsrt, iqty, etqty, tdqty, edqty, spdqty, otltdqty, itqty, dambsarf) VALUES ");

                for (int i = 0; i < damData.Rows.Count; i++)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    string obsdh = damData.Rows[i]["obsdh"].ToString();
                    string rwl = damData.Rows[i]["rwl"].ToString();
                    string ospilwl = damData.Rows[i]["ospilwl"].ToString();
                    string rsqty = damData.Rows[i]["rsqty"].ToString();
                    string rsrt = damData.Rows[i]["rsrt"].ToString();
                    string iqty = damData.Rows[i]["iqty"].ToString();
                    string etqty = damData.Rows[i]["etqty"].ToString();
                    string tdqty = damData.Rows[i]["tdqty"].ToString();
                    string edqty = damData.Rows[i]["edqty"].ToString();
                    string spdqty = damData.Rows[i]["spdqty"].ToString();
                    string otltdqty = damData.Rows[i]["otltdqty"].ToString();
                    string itqty = damData.Rows[i]["itqty"].ToString();
                    string dambsarf = damData.Rows[i]["dambsarf"].ToString();

                    query.Append(string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')", damCD, obsdh, rwl, ospilwl, rsqty, rsrt, iqty, etqty, tdqty, edqty, spdqty, otltdqty, itqty, dambsarf));
                }

                var command = new NpgsqlCommand(query.ToString(), _global.NpgSQLconn);
                command.ExecuteNonQuery();
                //transaction.Commit();

                sw.Stop();
                WriteToStatus(sw.ElapsedMilliseconds.ToString());

                return true;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                return false;
            }
        }

        private bool DataInsert2(string damCD, DataTable damData)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //NpgsqlTransaction tran = _global.NpgSQLconn.BeginTransaction();

            try
            {
                for (int i = 0; i < damData.Rows.Count; i++)
                {
                    string Query = string.Format("INSERT INTO drought.tb_wamis_mnhrdata (damcd, obsdh, rwl, ospilwl, rsqty, rsrt, iqty, etqty, tdqty, edqty, spdqty, otltdqty, itqty, dambsarf) " +
                    " VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')"
                    , damCD, damData.Rows[i]["obsdh"].ToString(), damData.Rows[i]["rwl"].ToString(), damData.Rows[i]["ospilwl"].ToString()
                    , damData.Rows[i]["rsqty"].ToString(), damData.Rows[i]["rsrt"].ToString(), damData.Rows[i]["iqty"].ToString()
                    , damData.Rows[i]["etqty"].ToString(), damData.Rows[i]["tdqty"].ToString(), damData.Rows[i]["edqty"].ToString()
                    , damData.Rows[i]["spdqty"].ToString(), damData.Rows[i]["otltdqty"].ToString(), damData.Rows[i]["itqty"].ToString()
                    , damData.Rows[i]["dambsarf"].ToString()) ;

                    StringNonQuery(Query, _global.NpgSQLconn);
                }

                //tran.Commit();

                sw.Stop();
                WriteToStatus(sw.ElapsedMilliseconds.ToString());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

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

        private bool DataInsert(string damCD, DataTable damData)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var transaction = _global.NpgSQLconn.BeginTransaction();

                var sql = "INSERT INTO drought.tb_wamis_mnhrdata (damcd, obsdh, rwl, ospilwl, rsqty, rsrt, iqty, etqty, tdqty, edqty, spdqty, otltdqty, itqty, dambsarf) " +
                    " VALUES (@damcd, @obsdh, @rwl, @ospilwl, @rsqty, @rsrt, @iqty, @etqty, @tdqty, @edqty, @spdqty, @otltdqty, @itqty, @dambsarf)";

                var command = new NpgsqlCommand(sql, _global.NpgSQLconn, transaction);

                for (int i = 0; i < damData.Rows.Count; i++)
                {
                    command.Parameters.AddWithValue("damcd", damCD);
                    command.Parameters.AddWithValue("obsdh", damData.Rows[i]["obsdh"].ToString());
                    command.Parameters.AddWithValue("rwl", damData.Rows[i]["rwl"].ToString());
                    command.Parameters.AddWithValue("ospilwl", damData.Rows[i]["ospilwl"].ToString());
                    command.Parameters.AddWithValue("rsqty", damData.Rows[i]["rsqty"].ToString());
                    command.Parameters.AddWithValue("rsrt", damData.Rows[i]["rsrt"].ToString());
                    command.Parameters.AddWithValue("iqty", damData.Rows[i]["iqty"].ToString());
                    command.Parameters.AddWithValue("etqty", damData.Rows[i]["etqty"].ToString());
                    command.Parameters.AddWithValue("tdqty", damData.Rows[i]["tdqty"].ToString());
                    command.Parameters.AddWithValue("edqty", damData.Rows[i]["edqty"].ToString());
                    command.Parameters.AddWithValue("spdqty", damData.Rows[i]["spdqty"].ToString());
                    command.Parameters.AddWithValue("otltdqty", damData.Rows[i]["otltdqty"].ToString());
                    command.Parameters.AddWithValue("itqty", damData.Rows[i]["itqty"].ToString());
                    command.Parameters.AddWithValue("dambsarf", damData.Rows[i]["dambsarf"].ToString());

                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                transaction.Commit();

                sw.Stop();
                WriteToStatus(sw.ElapsedMilliseconds.ToString());

                return true;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                return false;
            }
        }

        #region [Message 함수]
        private void WriteToStatus(string message)
        {
            Application.DoEvents();

            if (this.listStatus.InvokeRequired)
            {
                WriteToStatusCallback d = new WriteToStatusCallback(WriteToStatus);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                if (listStatus.Items.Count > 200)
                {
                    listStatus.Items.Remove(listStatus.Items.Count);
                }
                listStatus.Items.Insert(0, DateTime.Now + " - " + message);

                GMLogHelper.WriteLog(message);
            }

            Application.DoEvents();
        }


        #endregion
    }
}
