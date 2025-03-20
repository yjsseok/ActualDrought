using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.XtraSplashScreen;
using Npgsql;
using OpenAPI.Controls;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace OpenAPI_DataCollecter
{
    public partial class frmWAMIS_mndtdata : Form
    {
        public Global _global {  get; set; }

        #region [Delegate]
        delegate void WriteToStatusCallback(string message);    //스레드교착
        #endregion

        public frmWAMIS_mndtdata()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmWAMIS_mndtdata_Load(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.dtpStart.Value = new DateTime(1970, 01, 01);
            this.dtpEnd.Value = new DateTime(2024, 12, 31);
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnSearch":
                    Search_DamDailyData();
                    break;
                default:
                    break;
            }
        }

        private void Search_DamDailyData()
        {
            WamisAPIService apiService = new WamisAPIService();
            WamisParamObj paramObj = new WamisParamObj("mn_dtdata"); // 파라미터 객체
            DataTable rtnTable = new DataTable();         // 저장할 테이블

            try
            {
                DateTime stDate = this.dtpStart.Value;
                DateTime edDate = this.dtpEnd.Value;

                for (DateTime dt = stDate; dt <= edDate; dt = dt.AddYears(3))
                {
                    DateTime Search_stDate = new DateTime(dt.Year, 1, 1);
                    DateTime Search_edDate = new DateTime(dt.Year+2, 12, 31);

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

                            if (DataInsert(damCD, damData) == true)
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
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(string damCD, DataTable damData)
        {
            try
            {
                var transaction = _global.NpgSQLconn.BeginTransaction();

                var sql = "INSERT INTO drought.tb_wamis_mndtdata (damcd, obsymd, rwl, iqty, tdqty, edqty, spdqty, otltdqty, itqty, rf) VALUES (@damcd, @obsymd, @rwl, @iqty, @tdqty, @edqty, @spdqty, @otltdqty, @itqty, @rf)";

                var command = new NpgsqlCommand(sql, _global.NpgSQLconn, transaction);

                for (int i = 0; i < damData.Rows.Count; i++)
                {
                    command.Parameters.AddWithValue("damcd", damCD);
                    command.Parameters.AddWithValue("obsymd", damData.Rows[i]["obsymd"].ToString());
                    command.Parameters.AddWithValue("rwl", damData.Rows[i]["rwl"].ToString());
                    command.Parameters.AddWithValue("iqty", damData.Rows[i]["iqty"].ToString());
                    command.Parameters.AddWithValue("tdqty", damData.Rows[i]["tdqty"].ToString());
                    command.Parameters.AddWithValue("edqty", damData.Rows[i]["edqty"].ToString());
                    command.Parameters.AddWithValue("spdqty", damData.Rows[i]["spdqty"].ToString());
                    command.Parameters.AddWithValue("otltdqty", damData.Rows[i]["otltdqty"].ToString());
                    command.Parameters.AddWithValue("itqty", damData.Rows[i]["itqty"].ToString());
                    command.Parameters.AddWithValue("rf", damData.Rows[i]["rf"].ToString());

                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                transaction.Commit();
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
