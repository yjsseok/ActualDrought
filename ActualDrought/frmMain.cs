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

namespace ActualDrought
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
                this.WriteToStatus("Database 연결 성공");
            }
            else
            {
                this.WriteToStatus("Database 연결 실패");
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

        // UI를 안전하게 업데이트하는 메서드
        private void WriteToStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteToStatus(message)));
            }
            else
            {
                listStatus.Items.Add(string.Format("{0}-{1}", DateTime.Now, message)); // listBox1에 메시지를 추가 (예: 로그 출력)
            }
        }
    }
}
