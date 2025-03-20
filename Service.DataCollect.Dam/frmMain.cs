using Npgsql;
using OpenAPI.Controls;
using OpenAPI.DataServices;
using OpenAPI.Model;
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

namespace Service.DataCollect.Dam
{
    public partial class frmMain : Form
    {
        #region [Delegate]
        delegate void deleOpenAPI_Wamis_mndtdata_Caller();

        private Thread thOpenAPI_WAMIS_mnhrdata { get; set; }
        private Thread thOpenAPI_WAMIS_mnhrdata_Period { get; set; }
        private Thread thOpenAPI_WAMIS_mnhrdata_Result { get; set; }
        #endregion

        #region [WorkerThread]
        private EventWaitHandle eventWaitHandle = new AutoResetEvent(false);
        private readonly object locker = new object();
        private Queue<List<DamHRData>> OpenAPI_WAMIS_mnhrdata_ResultQueue = new Queue<List<DamHRData>>();
        #endregion

        #region [Variables]
        public Global _global { get; set; }
        private bool isServiceRunning { get; set; }
        #endregion

        #region [Initialize]
        public frmMain()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            InitializeLogNBuild();
            InitializeVariables();

            //Site를 읽어오는 부분을 .csv 파일로 처리하는 부분도 필요
            InitializeSites();

            if (InitializeDatabase() == true)
            {

            }
            else
            {

            }
        }

        private void InitializeLogNBuild()
        {
            //Log설정
            GMLogManager.ConfigureLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"));

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

            _global.startDate = new DateTime(Config.StartDate, 1, 1);
            _global.endDate = new DateTime(Config.EndDate, 12, 31);
        }

        private void InitializeSites()
        {
            WamisAPIController apiController = new WamisAPIController();
            DataTable dt = new DataTable();
            dt = apiController.GetDamSiteData("mn_dammain");

            List<DamSiteInformation> listDam = new List<DamSiteInformation>();
            listDam = BizCommon.ConvertDataTableToList<DamSiteInformation>(dt);
            _global.listDams = listDam;

            this.WriteStatus(string.Format("초기화 : {0}개의 댐 Loading", listDam.Count));
        }

        private bool InitializeDatabase()
        {
            string dbIP = Config.dbIP;
            string dbName = Config.dbName;
            string dbPort = Config.dbPort;
            string dbId = Config.dbId;
            string dbPassword = Config.dbPassword;

            if (PostgreConnectionDB(dbIP, dbName, dbPort, dbId, dbPassword) == true)
            {
                this.WriteStatus("Database 연결 성공");
                return true;
            }
            else
            {
                this.WriteStatus("Database 연결 실패");
                return false;
            }
        }

        private bool PostgreConnectionDB(string dbIP, string dbName, string dbPort, string dbId, string dbPassword)
        {
            try
            {
                string strConn = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                        dbIP, dbPort, dbId, dbPassword, dbName);

                NpgsqlConnection NpgSQLconn = new NpgsqlConnection(strConn);
                NpgSQLconn.Open();

                NpgSQLconn.Close();
                return true;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                return false;
            }

        }

        #endregion

        #region [Service 함수]
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isServiceRunning) // 서비스가 실행 중이 아닐 경우
            {
                bool success = ServiceStart();

                if (success)
                {
                    GMLogHelper.WriteLog("Service started successfully.");
                    btnStart.Enabled = false; // Start 버튼 비활성화
                    btnStop.Enabled = true;  // Stop 버튼 활성화
                }
                else
                {
                    GMLogHelper.WriteLog("Failed to start the service.");
                }
            }
            else
            {
                GMLogHelper.WriteLog("Service is already running.");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isServiceRunning) // 서비스가 실행 중인 경우
            {
                ServiceStop();
                MessageBox.Show("Service stopped successfully.");
                btnStart.Enabled = true;  // Start 버튼 활성화
                btnStop.Enabled = false; // Stop 버튼 비활성화
            }
            else
            {
                MessageBox.Show("Service is not running.");
            }
        }

        private bool ServiceStart()
        {
            try
            {
                //WAMIS
                #region [WAMIS mnhrdata 자료수집 구동]
                //임시로 리얼타임 돌리지 않음
                _global.RealTimeUse = false;

                //////////////////JS
                if (_global.RealTimeUse == true)
                {
                    thOpenAPI_WAMIS_mnhrdata = new Thread(OpenAPI_Wamis_mndtdata_AutoCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_mnhrdata.Start();
                }
                #endregion//////////////////JS//////////////////JS

                #region [WAMIS mnhrdata 자료수집 기간]
                if (_global.PeriodUse == true)
                {
                    thOpenAPI_WAMIS_mnhrdata_Period = new Thread(OpenAPI_WAMIS_mnhrdata_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_mnhrdata_Period.Start();
                }
                #endregion

                #region [WAMIS mnhrdata 결과처리]
                thOpenAPI_WAMIS_mnhrdata_Result = new Thread(OpenAPI_WAMIS_mnhrdata_ResultCaller)
                {
                    IsBackground = true
                };
                thOpenAPI_WAMIS_mnhrdata_Result.Start();
                #endregion

                isServiceRunning = true; // 서비스 실행 상태 설정
                return true;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                return false;
            }


        }

        #endregion







        /// <summary>
        /// Wamis 댐수문정보 일자료 수집서비스 호출 Caller
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OpenAPI_Wamis_mndtdata_AutoCaller()
        {
            /// 10분 단위 호출
            int nTimeGap = 1000 * Config.OpenAPI_Wamis_mndtdata_Second;
            deleOpenAPI_Wamis_mndtdata_Caller deleService_Wamis_mndtdata = new deleOpenAPI_Wamis_mndtdata_Caller(this.OpenAPI_Wamis_mndtdata_Service
                );

            while (true)
            {
                IAsyncResult ar = deleService_Wamis_mndtdata.BeginInvoke(null, null);
                /// nTimeGap 만큼 Sleep
                Thread.Sleep(nTimeGap);
            }
        }

        private void OpenAPI_Wamis_mndtdata_Service()
        {
            this.WriteStatus("Wamis_mndtdata Module Start");
        }

        private void ServiceStop()
        {
            //try
            //{
            //    if (th)
            //    {

            //    }
            //    // SWMM 실행 스레드 종료
            //    if (thOpenAPI_KMA_ASOS != null && thOpenAPI_KMA_ASOS.IsAlive)
            //    {
            //        thOpenAPI_KMA_ASOS.Abort();
            //    }

            //    isServiceRunning = false; // 서비스 실행 상태 해제
            //}
            //catch (Exception ex)
            //{
            //    GMLogHelper.WriteLog($"Error stopping service: {ex.Message}");
            //}
        }

        private void OpenAPI_WAMIS_mnhrdata_PeriodCaller()
        {
            try
            {
                DateTime stDate = _global.startDate;
                DateTime edDate = _global.endDate;

                foreach (DamSiteInformation dam in _global.listDams)
                {
                    for (DateTime dt = stDate; dt <= edDate; dt = dt.AddMonths(6))
                    {
                        /*      DateTime Search_stDate = new DateTime(dt.Year, dt.Month, 1, 1, 0, 0);
                              int lastday = DateTime.DaysInMonth(dt.Year, dt.Month + 5);
                              DateTime Search_edDate = new DateTime(dt.Year, dt.Month + 5, lastday, 23, 59, 0);
                        */
                        DateTime Search_stDate = new DateTime(dt.Year, dt.Month, 1, 1, 0, 0);
                        DateTime tempDate = dt.AddMonths(5);
                        int lastday = DateTime.DaysInMonth(tempDate.Year, tempDate.Month);
                        DateTime Search_edDate = new DateTime(tempDate.Year, tempDate.Month, lastday, 23, 59, 0);




                        List<DamHRData> damDatas = WAMIS_Controller.GetDamHrData(dam.damcd, Search_stDate, Search_edDate);

                        if (damDatas != null)
                        {
                            EnqueueOpenAPIWAMISDamHrResult(damDatas);
                        }
                    }
                }
                
            }
            catch (Exception)
            {

                throw;
            }
            //foreach (DamSiteInformation dam in _global.listDams)
            //{
            //    for
            //}
        }

        private void EnqueueOpenAPIWAMISDamHrResult(List<DamHRData> damDatas)
        {
            //결과 파일명 Queue에 넣기
            lock (locker) OpenAPI_WAMIS_mnhrdata_ResultQueue.Enqueue(damDatas);

            //결과 처리하도록 이벤트 발생
            eventWaitHandle.Set();
        }

        private void OpenAPI_WAMIS_mnhrdata_ResultCaller()
        {
            WriteStatus(string.Format("WAMIS Dam HR Result Caller 실행"));

            while (true)
            {
                List<DamHRData> resultList = new List<DamHRData>();

                lock (locker)
                {
                    if (OpenAPI_WAMIS_mnhrdata_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_WAMIS_mnhrdata_ResultQueue.Dequeue();
                        // If file name is null then stop worker thread
                        if (resultList.Count == 0) return;
                    }
                }

                if (resultList.Count > 0)
                {
                    //결과 저장 로직
                    OpenAPIWAMISDamHRResultInsertProcess(resultList);
                }
                else
                {
                    //파일명 없을때 Wait Signal
                    eventWaitHandle.WaitOne();
                }
            }
        }

        private void OpenAPIWAMISDamHRResultInsertProcess(List<DamHRData> resultList)
        {
            string sDate = resultList.First().obsdh;
            string eDate = resultList.Last().obsdh;
            string damcd = resultList.First().damcd;

            List<DamHRData> DamHrDatas_DB = new List<DamHRData>();
            DamHrDatas_DB = NpgSQLService.GetDailyDatasFromOpenAPIWAMISDamHrData(damcd, sDate, eDate);

            //Database와 비교하여 Database에 없는것 입력
            List<DamHRData> addDatas = resultList.Where(current => !DamHrDatas_DB.Any(db => db.obsdh == current.obsdh && db.damcd == current.damcd)).ToList();

            //Bulk Insert 실행
            WriteStatus(string.Format("Insert process...{0} ea", addDatas.Count));

            if (NpgSQLService.BulkInsert_WAMISDamHrDatas(addDatas) == true)
            {
                WriteStatus(string.Format("Database Insert... {0}:site {1}:Date {2}:Datas Success", damcd, sDate.Substring(0, 4), addDatas.Count));
            }
            else
            {
                WriteStatus(string.Format("Database Insert... {0}:site {1}:Datas fail", damcd, addDatas.Count));
            }
        }

        #region [Write]
        // UI를 안전하게 업데이트하는 메서드
        private void WriteStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteStatus(message)));
            }
            else
            {
                listStatus.Items.Add(string.Format("{0}-{1}", DateTime.Now, message)); // listBox1에 메시지를 추가 (예: 로그 출력)
            }
        }

        #endregion

        
    }
}
