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
using System.Security.Policy;
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
        private volatile bool _shouldStop = false;
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
                _logger.Info("서비스 시작 중...", "Service");
               
                // WAMIS 실시간 데이터 수집 스레드
                if (_global.RealTimeUse == true && _global.PeriodUse != true)
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
            // 10분 단위 호출
            int nTimeGap = 1000 * Config.OpenAPI_Wamis_mndtdata_Second;
            deleOpenAPI_Wamis_mndtdata_Caller deleService_Wamis_mndtdata =
                new deleOpenAPI_Wamis_mndtdata_Caller(this.OpenAPI_Wamis_mndtdata_Service);

            while (!_shouldStop) // 종료 플래그 확인
            {
                IAsyncResult ar = deleService_Wamis_mndtdata.BeginInvoke(null, null);
                // nTimeGap 만큼 Sleep
                Thread.Sleep(nTimeGap);
            }
        }

        private void OpenAPI_Wamis_mndtdata_Service()
        {
            try
            {
                this.WriteStatus("Wamis_mndtdata Module Start");

                // DB에서 최종 데이터 일자 조회
                DateTime startDate;
                DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_WAMIS_mnhrdata();

                // 최종 데이터가 없거나 오류 발생 시 기본값 설정 (30일 전)
                if (lastDate == DateTime.MinValue)
                {
                    startDate = DateTime.Now.AddDays(-30);
                }
                else
                {
                    // 마지막 데이터 다음날부터 시작
                    startDate = lastDate.AddDays(0);
                    // 마지막 데이터 다음날이 오늘보다 늦은 경우 작업 취소
                    if (startDate > DateTime.Now)
                    {
                        this.WriteStatus(string.Format("최종 데이터({0})가 오늘({1})데이터 입니다. 데이터 수집을 건너뜁니다.",
                            lastDate.ToString("yyyy-MM-dd HH:mm"), DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
                        return; // 메서드 종료
                    }
                }
                // 종료일은 오늘 날짜로 설정
                DateTime endDate = DateTime.Now;

                // WAMIS API 기본 URL 설정
                string serviceURL = "http://www.wamis.go.kr:8080/wamis/openapi/wkd/mn_hrdata";
                string authKey = "b4568bbc61dabc1ce232c94d538f9f7d45229c1620";

                // 모든 댐에 대해 데이터 수집
                foreach (DamSiteInformation dam in _global.listDams)
                {
                    if (_shouldStop) break; // 종료 플래그 확인

                    this.WriteStatus(string.Format("{0} 댐 데이터 수집 시작 ({1} ~ {2})",
                        dam.damnm, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd")));

                    // API 요청 파라미터 설정 - 날짜 형식 명확하게 지정
                    string parameters = string.Format("?damcd={0}&startdt={1}&enddt={2}&authKey={3}",
                        dam.damcd,
                        startDate.ToString("yyyyMMdd"),
                        endDate.ToString("yyyyMMdd"),
                        authKey);

                    // 전체 URI 구성
                    Uri uri = new Uri(serviceURL + parameters);

                    // WAMIS API 호출하여 댐 수문 데이터 가져오기
                    List<DamHRData> damDatas = WAMIS_Controller.GetDamHrData(dam.damcd, startDate, endDate);

                    if (damDatas != null && damDatas.Count > 0)
                    {
                        this.WriteStatus(string.Format("수집된 {0} 댐 데이터: {1}개", dam.damnm, damDatas.Count));
                        EnqueueOpenAPIWAMISDamHrResult(damDatas);
                    }
                    else
                    {
                        this.WriteStatus(string.Format("{0} 댐 데이터 없음", dam.damnm));
                    }

                    // 연속 API 호출 시 서버 부하 방지를 위한 대기
                    Thread.Sleep(1000);
                }

                this.WriteStatus("Wamis_mndtdata Module End");
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));
                this.WriteStatus(string.Format("Wamis_mndtdata Module Error: {0}", ex.Message));
            }
        }

        private void ServiceStop()
        {
            try
            {
                // 종료 플래그 설정
                _shouldStop = true;

                // 대기 중인 스레드 깨우기
                eventWaitHandle.Set();

                // 각 스레드가 종료될 때까지 대기
                if (thOpenAPI_WAMIS_mnhrdata != null && thOpenAPI_WAMIS_mnhrdata.IsAlive)
                    thOpenAPI_WAMIS_mnhrdata.Join(100);

                if (thOpenAPI_WAMIS_mnhrdata_Period != null && thOpenAPI_WAMIS_mnhrdata_Period.IsAlive)
                    thOpenAPI_WAMIS_mnhrdata_Period.Join(100);

                if (thOpenAPI_WAMIS_mnhrdata_Result != null && thOpenAPI_WAMIS_mnhrdata_Result.IsAlive)
                    thOpenAPI_WAMIS_mnhrdata_Result.Join(100);

                isServiceRunning = false; // 서비스 실행 상태 해제
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("Error stopping service: {0}", ex.Message));
            }
        }

        private void OpenAPI_WAMIS_mnhrdata_PeriodCaller()
        {
            try
            {
                DateTime stDate = _global.startDate;
                DateTime edDate = _global.endDate;

                foreach (DamSiteInformation dam in _global.listDams)
                {
                    if (_shouldStop) break; // 종료 플래그 확인

                    for (DateTime dt = stDate; dt <= edDate; dt = dt.AddMonths(6))
                    {
                        if (_shouldStop) break; // 종료 플래그 확인

                        DateTime Search_stDate = new DateTime(dt.Year, dt.Month, 1, 1, 0, 0);

                        // 다음 6개월 날짜 계산
                        DateTime tempDate = dt.AddMonths(5);

                        // 종료일을 초과하지 않도록 조정
                        if (tempDate > edDate)
                            tempDate = edDate;

                        int lastday = DateTime.DaysInMonth(tempDate.Year, tempDate.Month);
                        DateTime Search_edDate;

                        // 마지막 반복에서는 정확한 종료일 사용
                        if (tempDate.Month == edDate.Month && tempDate.Year == edDate.Year)
                            Search_edDate = new DateTime(tempDate.Year, tempDate.Month, edDate.Day, 23, 59, 0);
                        else
                            Search_edDate = new DateTime(tempDate.Year, tempDate.Month, lastday, 23, 59, 0);

                        List<DamHRData> damDatas = WAMIS_Controller.GetDamHrData(dam.damcd, Search_stDate, Search_edDate);

                        if (damDatas != null && damDatas.Count > 0)
                        {
                            this.WriteStatus(string.Format("기간별 수집된 {0} 댐 데이터: {1}개 ({2}~{3})",
                                dam.damnm, damDatas.Count,
                                Search_stDate.ToString("yyyy-MM-dd"),
                                Search_edDate.ToString("yyyy-MM-dd")));
                            EnqueueOpenAPIWAMISDamHrResult(damDatas);
                        }

                        // 연속 API 호출 시 서버 부하 방지를 위한 대기
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));
                this.WriteStatus(string.Format("WAMIS_mnhrdata_Period Error: {0}", ex.Message));
            }
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

            while (!_shouldStop) // 종료 플래그 확인
            {
                List<DamHRData> resultList = new List<DamHRData>();

                lock (locker)
                {
                    if (OpenAPI_WAMIS_mnhrdata_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_WAMIS_mnhrdata_ResultQueue.Dequeue();

                        // 빈 리스트면 다음 반복으로
                        if (resultList.Count == 0)
                            continue;
                    }
                }

                if (resultList.Count > 0)
                {
                    // 결과 저장 로직
                    OpenAPIWAMISDamHRResultInsertProcess(resultList);
                }
                else
                {
                    // 파일명 없을때 Wait Signal (최대 5초 대기 후 종료 플래그 확인)
                    eventWaitHandle.WaitOne(100);
                }
            }

            WriteStatus(string.Format("WAMIS Dam HR Result Caller 종료"));
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
                try
                {
                    // 로그 메시지 추가
                    listStatus.Items.Add(string.Format("{0}-{1}", DateTime.Now, message));

                    // 스크롤을 최신 항목으로 이동
                    listStatus.TopIndex = listStatus.Items.Count - 1;

                    // 항목이 너무 많아지면 오래된 항목 제거
                    if (listStatus.Items.Count > 10000)
                    {
                        listStatus.Items.RemoveAt(0);
                    }

                    // UI 업데이트 강제
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    GMLogHelper.WriteLog(string.Format("UI 업데이트 오류: {0}", ex.Message));
                }
            }
        }

        #endregion

        private void periodSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm(typeof(frmConfig), true, true);
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
                //if (frmTarget is frmLogin)
                //{
                //    if (frmTarget.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                //    {

                //    }
                //    return DialogResult.None;
                //}
                //else
                //{
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
        #endregion
    }
}
