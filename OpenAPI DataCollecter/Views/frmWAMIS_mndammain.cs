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
    public partial class frmWAMIS_mndammain : Form
    {
        public Global _global { get; set; }
        public List<mndammain> damInfo { get; set; }

        public frmWAMIS_mndammain()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmWAMIS_mndammain_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.damInfo = new List<mndammain>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnDamInfo_Req":
                    Request_DamInfo();
                    break;
                case "btnSaveDatabase":
                    DataInsert();
                    break;
            }
        }

        private void DataInsert()
        {
            try
            {
                var transaction = _global.NpgSQLconn.BeginTransaction();

                var sql = "INSERT INTO drought.tb_wamis_mndammain (damcd, damnm, bbsncd, sbsncd, bbsnnm, mggvnm) VALUES (@damcd, @damnm, @bbsncd, @sbsncd, @bbsnnm, @mggvnm)";

                var command = new NpgsqlCommand(sql, _global.NpgSQLconn, transaction);

                foreach (var mndammain in this.damInfo)
                {
                    command.Parameters.AddWithValue("damcd", mndammain.damcd);
                    command.Parameters.AddWithValue("damnm", mndammain.damnm);
                    command.Parameters.AddWithValue("bbsncd", mndammain.bbsncd);
                    command.Parameters.AddWithValue("sbsncd", mndammain.sbsncd);
                    command.Parameters.AddWithValue("bbsnnm", mndammain.bbsnnm);
                    command.Parameters.AddWithValue("mggvnm", mndammain.mggvnm);

                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                throw;
            }
            
        }

        private void Request_DamInfo()
        {
            SplashScreenManager.ShowForm(typeof(frmWait));
            SplashScreenManager.Default.SetWaitFormCaption("댐 제원 조회중...");
            SplashScreenManager.Default.SetWaitFormDescription("Searching...");

            try
            {
                WamisAPIService apiService = new WamisAPIService();
                DataTable dtDam = new DataTable();
                dtDam = apiService.getList("mn_dammain", "");
                this.damInfo = ConvertDTtoList(dtDam);
                this.ultraGrid1.DataSource = dtDam; ;
                //WamisAPIController apiController = new WamisAPIController();

                //DataTable dtDam = new DataTable();
                //dtDam = apiController.Get_mn_dammain_All_DataTable("mn_dammain");
                //this.ultraGrid1.DataSource = dtDam;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                SplashScreenManager.CloseForm(false);
            }
        }

        private List<mndammain> ConvertDTtoList(DataTable dtDam)
        {
            List<mndammain> listDamData = new List<mndammain>();

            for (int i = 0; i < dtDam.Rows.Count; i++)
            {
                mndammain addData = new mndammain();

                addData.damcd = dtDam.Rows[i]["damcd"].ToString();
                addData.damnm = dtDam.Rows[i]["damnm"].ToString();
                addData.bbsncd = dtDam.Rows[i]["bbsncd"].ToString();
                addData.sbsncd = dtDam.Rows[i]["sbsncd"].ToString();
                addData.bbsnnm = dtDam.Rows[i]["bbsnnm"].ToString();
                addData.mggvnm = dtDam.Rows[i]["mggvnm"].ToString();

                listDamData.Add(addData);
            }

            return listDamData;
        }
    }
}
