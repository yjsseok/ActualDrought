using Npgsql;
using OpenAPI.Controls;
using OpenAPI.DataServices;
using OpenAPI.Model;
using Service.JSlogger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace Service.DataCollect.drghtdamoper
{
    public partial class frmMain : Form
    {
        #region [Thread 변수]
        delegate void deleWAMISFlow_AutoCaller();
        private Thread thOpenAPI_WAMIS_Flow { get; set; }
        private Thread thOpenAPI_WAMIS_Period { get; set; }
        private Thread thOpenAPI_WAMIS_Result { get; set; }
        #endregion

        #region [WorkerThread]
        private EventWaitHandle eventWaitHandle = new AutoResetEvent(false);
        private readonly object locker = new object();
        private Queue<List<FlowData>> OpenAPI_WAMIS_Flow_ResultQueue = new Queue<List<FlowData>>();
        #endregion

        #region [Variables]
        public Global _global { get; set; }
        private bool isServiceRunning { get; set; }
        private volatile bool _shouldStop = false;
        private LogManager _logger; // 로그 관리자 인스턴스
        #endregion

        #region [Initialize]
        public frmMain()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // 로그 관리자 초기화
            _logger = LogManager.GetInstance();
            _logger.Initialize(listStatus, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"), "drghtdamoper");
            _logger.Info("애플리케이션 시작", "System");

            InitializeLogNBuild();
            InitializeVariables();
            InitializeSites();

            if (InitializeDatabase() == true)
            {
                _logger.Info("데이터베이스 초기화 성공", "Database");
            }
            else
            {
                _logger.Error("데이터베이스 초기화 실패", "Database");
            }
        }

        private void InitializeLogNBuild()
        {
            // 버전 정보 표시
            _logger.Info($"버전: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}", "System");
            this.Text += string.Format(" V{0}.{1}.{2}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);
        }

        private void InitializeVariables()
        {
            this.isServiceRunning = false;
            _global.RealTimeUse = BizCommon.BoolConvert(Config.RealTimeUse);
            _global.PeriodUse = BizCommon.BoolConvert(Config.PeriodUse);

        }

        private void InitializeSites()
        {
            _logger.Info("drghtdamoper", "Initialize");
        }

        private bool InitializeDatabase()
        {
            _logger.Info("데이터베이스 연결 시도 중...", "Database");
            string dbIP = Config.dbIP;
            string dbName = Config.dbName;
            string dbPort = Config.dbPort;
            string dbId = Config.dbId;
            string dbPassword = Config.dbPassword;

            _logger.Debug($"DB 연결 정보: IP={dbIP}, DB={dbName}, Port={dbPort}, ID={dbId}", "Database");

            if (PostgreConnectionDB(dbIP, dbName, dbPort, dbId, dbPassword) == true)
            {
                _logger.Info("데이터베이스 연결 성공", "Database");
                return true;
            }
            else
            {
                _logger.Error("데이터베이스 연결 실패", "Database");
                return false;
            }
        }

        private bool PostgreConnectionDB(string dbIP, string dbName, string dbPort, string dbId, string dbPassword)
        {
            try
            {
                string strConn = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                    dbIP, dbPort, dbId, dbPassword, dbName);

                _logger.Debug("DB 연결 시도...", "Database");
                using (NpgsqlConnection NpgSQLconn = new NpgsqlConnection(strConn))
                {
                    NpgSQLconn.Open();
                    _logger.Debug("DB 연결 성공", "Database");
                    NpgSQLconn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "데이터베이스 연결 중 오류 발생", LogLevel.Error, "Database");
                return false;
            }
        }
        #endregion

        #region [Service 함수]
        private async void btnStart_Click(object sender, EventArgs e)
        {
            await CollectAndSaveDrghtDamOperData();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isServiceRunning) // 서비스가 실행 중인 경우
            {
                _logger.Info("서비스 중지 요청", "Service");
                _logger.Info("서비스가 중지되었습니다.", "Service");
                btnStart.Enabled = true; // Start 버튼 활성화
                btnStop.Enabled = false; // Stop 버튼 비활성화
            }
            else
            {
                _logger.Warning("서비스가 실행 중이 아닙니다.", "Service");
            }
        }

        // 전체 실행 메서드
        private async Task CollectAndSaveDrghtDamOperData()
        {
            // damcd 리스트
            var damcdList = new List<string> {
        "2403201","1001210","2503220","2008101","2010101","1302210","2201231","2201230","3008110","4007210","2021110","3203110","2012101","3303110","2201220","2301210","4001110","2002111","1012110","4105210","2101210","2001110","2503210","2004101","2012210","3001110","2021210","2002110","5101110","4104610","4007110","1003110","5002201","2015110","1006110"
    };

            // 날짜 계산
            DateTime lastDate = NpgSQLService.GetLastObsDate_DrghtDamOper();
            string stDt = lastDate.AddDays(1).ToString("yyyyMMdd"); // 마지막 데이터 다음날
            string edDt = DateTime.Today.ToString("yyyyMMdd");      // 오늘

            var controller = new DrghtDamOperController();
            var allData = new List<DrghtDamOperData>();

            foreach (var damcd in damcdList)
            {
                var data = await controller.GetDamOperDataAsync(damcd, stDt, edDt);
                allData.AddRange(data);
            }

            bool result = NpgSQLService.BulkInsert_DrghtDamOperDatas(allData);
            MessageBox.Show(result ? $"{stDt}~{edDt} 데이터 저장 완료" : "저장 실패");
        }




        #endregion

        #region [메뉴]
        private void periodSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.Debug("기간 설정 메뉴 클릭", "UI");
            ShowForm(typeof(frmConfig), true, true);
        }
        #endregion

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
            try
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
                        if (frm.GetType() == type)
                        {
                            frm.Activate();
                            return DialogResult.None;
                        }
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
                    frmTarget.StartPosition = FormStartPosition.CenterParent;
                    return frmTarget.ShowDialog();
                }
                else
                {
                    frmTarget.Show();
                    return DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "폼 표시 오류", LogLevel.Error, "UI");
                return DialogResult.None;
            }
        }
        #endregion
    }
}
