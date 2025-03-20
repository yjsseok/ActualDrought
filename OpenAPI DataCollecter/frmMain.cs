using Npgsql;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace OpenAPI_DataCollecter
{
    public partial class frmMain : Form
    {
        public Global _global { get; set; }

        public frmMain()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text += string.Format(" V{0}.{1}.{2}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);

            GMLogManager.ConfigureLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"));

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            string dbIP = Config.dbIP;
            string dbName = Config.dbName;
            string dbPort = Config.dbPort;
            string dbId = Config.dbId;
            string dbPassword = Config.dbPassword;

            if (ConnectionDB(dbIP, dbName, dbPort, dbId, dbPassword) == true)
            {
                //this.WriteToStatus("Database 연결 성공");
            }
            else
            {
                //this.WriteToStatus("Database 연결 실패");
            }
        }

        private bool ConnectionDB(string dbIP, string dbName, string dbPort, string dbId, string dbPassword)
        {
            try
            {
                string strConn = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};Timeout=600",
                        dbIP, dbPort, dbId, dbPassword, dbName);

                this._global.NpgSQLconn = new NpgsqlConnection(strConn);
                this._global.NpgSQLconn.Open();

                return true;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                return false;
            }
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnWamis_DamInfo":
                    ShowForm(typeof(frmWAMIS_mndammain));
                    break;
                case "btnWamis_mnhrdata": //시자료
                    ShowForm(typeof(frmWAMIS_mnhrdata));
                    break;
                case "btnWamis_mndtdata": //일자료
                    ShowForm(typeof(frmWAMIS_mndtdata));
                    break;
                case "btnWamis_mnmndata":
                    break;
                case "btnModel_ModelKK":
                    ShowForm(typeof(frmModel_ModelKK));
                    break;
                case "btnModel_SGMT":
                    ShowForm(typeof(frmModel_ModelSGMT));
                    break;
                case "btnModel_SGHYD":
                    ShowForm(typeof(frmModel_ModelSGHYD));
                    break;
                case "btnModel_SGAGRPADDY":
                    ShowForm(typeof(frmModel_ModelSGAGRPADDY));
                    break;
                case "btnModel_SGRSRV":
                    ShowForm(typeof(frmModel_ModelSGRSRV));
                    break;
                case "btnModel_SGFLSanrm":
                    ShowForm(typeof(frmModel_ModelSGFLSanrm));
                    break;
                case "btnModel_SGFLS":
                    ShowForm(typeof(frmModel_ModelSGFLS));
                    break;
                case "btnModel_SoilMoisture":
                    ShowForm(typeof(frmModel_OBSRVN));
                    break;
                case "btnPDP_AgriDam":
                    ShowForm(typeof(frmPDP_AgriDam));
                    break;
            }
        }

        #region [FormShow 관련 함수들]

        private void ShowForm(Type type)
        {
            ShowForm(type, false, false);
        }

        private void ShowForm(Type type, bool isPopup)
        {
            ShowForm(type, isPopup, false);
        }

        /// <summary>
        /// 폼 띄우는 함수
        /// </summary>
        /// <param name="type">폼의종류</param>
        /// <param name="isPopup">팝업유무</param>
        /// <param name="isModal">모달유무</param>
        /// <returns></returns>
        private DialogResult ShowForm(Type type, bool isPopup, bool isModal)
        {
            if (type == null)
            {
                throw new ArgumentException("type is null");
            }

            // 팝업이 아니면 기존에 열려있는 폼 닫기
            if (!isPopup)
            {
                foreach (var frm in this.MdiChildren)
                {

                }
            }

            foreach (var frm in this.MdiChildren)
            {
                if (frm.GetType() == type)
                {
                    frm.Activate();
                    return DialogResult.None;
                }
            }

            Form frmTarget = Activator.CreateInstance(type) as Form;

            frmTarget.Owner = this;
            frmTarget.AutoScaleMode = AutoScaleMode.None;

            if (!isPopup)
            {
                frmTarget.MdiParent = this;
                frmTarget.WindowState = FormWindowState.Maximized;
                frmTarget.Show();
                return DialogResult.None;
            }

            if (isModal)
            {
                frmTarget.Owner = this;

                frmTarget.StartPosition = FormStartPosition.CenterParent;
                return frmTarget.ShowDialog();
                //}
            }
            else
            {
                //frmTarget.StartPosition = FormStartPosition.CenterParent;
                frmTarget.Show();
                return DialogResult.None;
            }
        }

        /// <summary>
        /// 기존생성폼 삭제함수
        /// </summary>
        private void DisposeForm()
        {
            if (this.MdiChildren.Count() > 0)
            {
                foreach (var frm in this.MdiChildren)
                {
                    frm.Close();
                }
            }
        }

        #endregion
    }
}
