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

namespace Service.DataCollect.Flow
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
            _logger.Initialize(listStatus, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"), "Flow");
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

            // frmConfig의 DTP에서 날짜 범위 가져오기
            frmConfig configForm = new frmConfig();
            _global.startDate = configForm.dtpStart.Value;
            _global.endDate = configForm.dtpEnd.Value;

            _logger.Debug($"설정 정보: 실시간 사용={_global.RealTimeUse}, 기간 사용={_global.PeriodUse}", "Config");
            _logger.Debug($"기간 설정: 시작일={_global.startDate:yyyy-MM-dd}, 종료일={_global.endDate:yyyy-MM-dd}", "Config");
        }

        private void InitializeSites()
        {
            _logger.Info("유량 관측지점 정보 초기화 중...", "Initialize");
            WamisAPIController apiController = new WamisAPIController();
            DataTable dt = new DataTable();
            dt = apiController.GetObsData("fl_obs");
            List<FlowSiteInformation> listObs = new List<FlowSiteInformation>();
            listObs = BizCommon.ConvertDataTableToList<FlowSiteInformation>(dt);
            _global.listFlowOBS = listObs;
            _logger.Info($"초기화 완료: {listObs.Count}개의 유량관측지점", "Initialize");
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
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isServiceRunning) // 서비스가 실행 중이 아닐 경우
            {
                _logger.Info("서비스 시작 요청", "Service");
                bool success = ServiceStart();
                if (success)
                {
                    _logger.Info("서비스가 성공적으로 시작되었습니다.", "Service");
                    btnStart.Enabled = false; // Start 버튼 비활성화
                    btnStop.Enabled = true; // Stop 버튼 활성화
                }
                else
                {
                    _logger.Error("서비스 시작에 실패했습니다.", "Service");
                }
            }
            else
            {
                _logger.Warning("서비스가 이미 실행 중입니다.", "Service");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isServiceRunning) // 서비스가 실행 중인 경우
            {
                _logger.Info("서비스 중지 요청", "Service");
                ServiceStop();
                _logger.Info("서비스가 중지되었습니다.", "Service");
                btnStart.Enabled = true; // Start 버튼 활성화
                btnStop.Enabled = false; // Stop 버튼 비활성화
            }
            else
            {
                _logger.Warning("서비스가 실행 중이 아닙니다.", "Service");
            }
        }

        private bool ServiceStart()
        {
            try
            {
                _shouldStop = false;

                #region [WAMIS Flow 자료수집 구동]
                if (_global.RealTimeUse == true)
                {
                    _logger.Info("실시간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_WAMIS_Flow = new Thread(OpenAPI_WAMIS_Flow_AutoCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_Flow.Start();
                }
                #endregion

                #region [WAMIS Flow 자료수집 기간]
                if (_global.PeriodUse == true)
                {
                    _logger.Info("기간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_WAMIS_Period = new Thread(OpenAPI_WAMIS_Flow_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_Period.Start();
                }
                #endregion

                #region [WAMIS Flow 결과처리]
                _logger.Info("결과 처리 스레드 시작", "Thread");
                thOpenAPI_WAMIS_Result = new Thread(OpenAPI_WAMIS_Flow_ResultCaller)
                {
                    IsBackground = true
                };
                thOpenAPI_WAMIS_Result.Start();
                #endregion

                isServiceRunning = true; // 서비스 실행 상태 설정
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 시작 중 오류 발생", LogLevel.Error, "Service");
                isServiceRunning = false; // 서비스 상태 플래그 해제
                return false;
            }
        }

        private void ServiceStop()
        {
            try
            {
                _logger.Info("서비스 중지 요청", "Service");
                _shouldStop = true;
                eventWaitHandle.Set();

                // 실시간 데이터 수집 스레드 종료
                if (thOpenAPI_WAMIS_Flow != null && thOpenAPI_WAMIS_Flow.IsAlive)
                {
                    _logger.Debug("실시간 데이터 수집 스레드 종료 중...", "Thread");
                    thOpenAPI_WAMIS_Flow.Join(1000);
                }

                // 기간별 데이터 수집 스레드 종료
                if (thOpenAPI_WAMIS_Period != null && thOpenAPI_WAMIS_Period.IsAlive)
                {
                    _logger.Debug("기간 데이터 수집 스레드 종료 중...", "Thread");
                    thOpenAPI_WAMIS_Period.Join(1000);
                }

                // 결과 처리 스레드 종료
                if (thOpenAPI_WAMIS_Result != null && thOpenAPI_WAMIS_Result.IsAlive)
                {
                    _logger.Debug("결과 처리 스레드 종료 중...", "Thread");
                    thOpenAPI_WAMIS_Result.Join(1000);
                }

                isServiceRunning = false; // 서비스 실행 상태 해제
                _logger.Info("모든 스레드가 종료되었습니다.", "Thread");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 중지 중 오류 발생", LogLevel.Error, "Service");
            }
        }

        private void OpenAPI_WAMIS_Flow_AutoCaller()
        {
            //구동시설 설정
            int nTimeGap = 1000 * Config.WAMIS_Flow_Auto_Caller_Second;
            _logger.Info($"실시간 데이터 수집 간격: {Config.WAMIS_Flow_Auto_Caller_Second}초", "AutoCaller");

            deleWAMISFlow_AutoCaller dele_WAMIS_Flow = new deleWAMISFlow_AutoCaller(OpenAPI_WAMIS_Flow_Service);

            while (!_shouldStop)
            {
                _logger.Debug("실시간 데이터 수집 호출 시작", "AutoCaller");
                IAsyncResult ar = dele_WAMIS_Flow.BeginInvoke(null, null);
                Thread.Sleep(nTimeGap);
            }

            _logger.Info("실시간 데이터 수집 스레드 종료", "AutoCaller");
        }

        private async void OpenAPI_WAMIS_Flow_Service()
        {
            try
            {
                _logger.Info("WAMIS Flow 실시간 데이터 수집 모듈 시작", "Service");

                // DB에서 최종 데이터 일자 조회
                DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_WAMIS_Flow();
                DateTime today = DateTime.Today;

                _logger.Info($"최종 데이터 일자: {lastDate:yyyy-MM-dd}, 오늘 날짜: {today:yyyy-MM-dd}", "Service");

                // 최종 데이터가 오늘 날짜보다 이후인 경우 작업 취소
                if (lastDate.Date >= today.Date)
                {
                    string message = $"최종 데이터({lastDate:yyyy-MM-dd})가 오늘자입니다. 데이터 수집을 건너뜁니다.";
                    _logger.Warning(message, "Service");
                    return; // 메서드 종료
                }

                string serviceURL = "http://www.wamis.go.kr:8080/wamis/openapi/wkw/flw_dtdata";
                string authKey = "b4568bbc61dabc1ce232c94d538f9f7d45229c1620";

                foreach (FlowSiteInformation obs in _global.listFlowOBS)
                {
                    if (_shouldStop) break;

                    _logger.Info($"[{obs.obsnm}] 관측소 데이터 수집 시작", "API");

                    int year = DateTime.Now.Year;
                    string parameters = $"?obscd={obs.obscd}&year={year}&authKey={authKey}";
                    Uri uri = new Uri(serviceURL + parameters);

                    // API 호출 시작 시간 기록
                    DateTime apiCallStart = DateTime.Now;

                    List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, year);

                    // API 호출 소요 시간 계산
                    TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                    _logger.LogPerformance($"[{obs.obsnm}] API 호출", (long)apiCallDuration.TotalMilliseconds);

                    if (flowData != null && flowData.Count > 0)
                    {
                        // 현재 날짜까지만 필터링
                        List<FlowData> filteredData = new List<FlowData>();
                        DateTime currentDate = DateTime.Now;

                        foreach (var data in flowData)
                        {
                            if (DateTime.TryParseExact(data.ymd, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dataDate))
                            {
                                if (dataDate <= currentDate)
                                {
                                    filteredData.Add(data);
                                }
                            }
                        }

                        _logger.Info($"수집된 [{obs.obsnm}] 관측소 데이터: {filteredData.Count}개 (필터링 전: {flowData.Count}개)", "API");

                        if (filteredData.Count > 0)
                        {
                            EnqueueOpenAPIWAMISFlowResult(filteredData);
                        }
                    }
                    else
                    {
                        _logger.Warning($"[{obs.obsnm}] 관측소 데이터 없음", "API");
                    }

                    // API 호출 간 딜레이 추가 (과도한 요청 방지)
                    Thread.Sleep(100);
                }

                _logger.Info("WAMIS Flow 실시간 데이터 수집 모듈 종료", "Service");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "WAMIS Flow 실시간 데이터 수집 모듈 오류", LogLevel.Error, "Service");
            }
        }

        private async void OpenAPI_WAMIS_Flow_PeriodCaller()
        {
            try
            {
                _logger.Info("WAMIS Flow 기간 조회 모듈 시작", "PeriodCaller");

                foreach (FlowSiteInformation obs in _global.listFlowOBS)
                {
                    if (_shouldStop) break;

                    // 설정된 기간에서 연도 범위 추출
                    int startYear = _global.startDate.Year;
                    int endYear = _global.endDate.Year;

                    // 설정된 기간과 관측소의 연도 범위 중 겹치는 부분만 처리
                    int processStartYear = Math.Max(obs.minYear, startYear);
                    int processEndYear = Math.Min(obs.maxYear, endYear);

                    _logger.Info($"[{obs.obsnm}] 관측소 기간 데이터 처리: {processStartYear}년 ~ {processEndYear}년", "PeriodCaller");

                    for (int i = processStartYear; i <= processEndYear; i++)
                    {
                        if (_shouldStop) break;

                        _logger.Info($"[{obs.obsnm}] 관측소 {i}년 데이터 수집 시작", "PeriodCaller");

                        // API 호출 시작 시간 기록
                        DateTime apiCallStart = DateTime.Now;

                        List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, i);

                        // API 호출 소요 시간 계산
                        TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                        _logger.LogPerformance($"[{obs.obsnm}] {i}년 API 호출", (long)apiCallDuration.TotalMilliseconds);

                        if (flowData != null)
                        {
                            // 데이터 생성 및 List에 저장
                            List<FlowData> dailyFlowDataList = new List<FlowData>();

                            // 해당 연도의 시작일과 종료일 설정
                            DateTime yearStartDate = new DateTime(i, 1, 1);
                            DateTime yearEndDate = new DateTime(i, 12, 31);

                            // 설정된 기간 내에서만 처리
                            DateTime periodStartDate = (i == startYear) ? _global.startDate : yearStartDate;
                            DateTime periodEndDate = (i == endYear) ? _global.endDate : yearEndDate;

                            // 현재 연도인 경우 오늘까지만 처리
                            if (i == DateTime.Now.Year)
                            {
                                periodEndDate = DateTime.Now < periodEndDate ? DateTime.Now : periodEndDate;
                                _logger.Debug($"[{obs.obsnm}] 관측소 {i}년 데이터 - 현재 날짜({periodEndDate:yyyy-MM-dd})까지만 처리", "PeriodCaller");
                            }
                            else
                            {
                                _logger.Debug($"[{obs.obsnm}] 관측소 {i}년 데이터 - {periodStartDate:yyyy-MM-dd}부터 {periodEndDate:yyyy-MM-dd}까지 처리", "PeriodCaller");
                            }

                            int totalDays = (periodEndDate - periodStartDate).Days + 1;
                            for (int j = 0; j < totalDays; j++)
                            {
                                DateTime currentDate = periodStartDate.AddDays(j);
                                string ymd = currentDate.ToString("yyyyMMdd");
                                FlowData dailyFlowData = new FlowData { obscd = obs.obscd, ymd = ymd, flw = double.NaN }; // 기본값으로 NaN 설정

                                if (flowData.Any(data => data.ymd == ymd))
                                    dailyFlowData.flw = flowData.FirstOrDefault(data => data.ymd == ymd).flw;

                                dailyFlowDataList.Add(dailyFlowData);
                            }

                            // DataQueue에 넣기
                            if (dailyFlowDataList.Count > 0)
                            {
                                _logger.Info($"[{obs.obsnm}] 관측소 {i}년 처리 데이터 수: {dailyFlowDataList.Count}개", "PeriodCaller");
                                EnqueueOpenAPIWAMISFlowResult(dailyFlowDataList);
                            }
                        }
                        else
                        {
                            _logger.Warning($"[{obs.obsnm}] 관측소 {i}년 데이터 없음", "PeriodCaller");
                        }

                        // API 호출 간 딜레이 추가 (과도한 요청 방지)
                        await Task.Delay(100);
                    }
                }

                _logger.Info("WAMIS Flow 기간 조회 모듈 종료", "PeriodCaller");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "WAMIS Flow 기간 조회 모듈 오류", LogLevel.Error, "PeriodCaller");
            }
        }

        private void EnqueueOpenAPIWAMISFlowResult(List<FlowData> dailyFlowDataList)
        {
            try
            {
                // 결과 파일명 Queue에 넣기
                _logger.Debug($"결과 큐에 {dailyFlowDataList.Count}개 데이터 추가 중", "Queue");
                lock (locker)
                {
                    OpenAPI_WAMIS_Flow_ResultQueue.Enqueue(dailyFlowDataList);
                }

                // 결과 처리하도록 이벤트 발생
                eventWaitHandle.Set();
                _logger.Debug("결과 처리 이벤트 발생", "Queue");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "결과 큐 추가 중 오류 발생", LogLevel.Error, "Queue");
            }
        }

        private void OpenAPI_WAMIS_Flow_ResultCaller()
        {
            _logger.Info("WAMIS Flow 결과 처리 모듈 시작", "ResultCaller");

            while (!_shouldStop)
            {
                List<FlowData> resultList = new List<FlowData>();
                bool hasData = false;

                lock (locker)
                {
                    if (OpenAPI_WAMIS_Flow_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_WAMIS_Flow_ResultQueue.Dequeue();
                        hasData = true;
                        _logger.Debug($"결과 큐에서 {resultList.Count}개 데이터 가져옴", "ResultCaller");
                    }
                }

                if (hasData && resultList.Count > 0)
                {
                    // 결과 저장 로직
                    OpenAPIWAMISFlowResultInsertProcess(resultList);
                }
                else
                {
                    // 파일명 없을때 Wait Signal (최대 100ms 대기)
                    eventWaitHandle.WaitOne(100);
                }
            }

            _logger.Info("WAMIS Flow 결과 처리 모듈 종료", "ResultCaller");
        }

        private void OpenAPIWAMISFlowResultInsertProcess(List<FlowData> resultList)
        {
            try
            {
                string sDate = resultList.First().ymd;
                string eDate = resultList.Last().ymd;
                string obsCD = resultList.First().obscd;

                _logger.Info($"[{obsCD}] 관측소 데이터 저장 준비: {resultList.Count}건 ({sDate} ~ {eDate})", "Database");

                // 기존 데이터 조회 시작 시간 기록
                DateTime dbQueryStart = DateTime.Now;

                List<FlowData> flowData_DB = NpgSQLService.GetDailyDatasFromOpenAPIWAMISFlow(obsCD, sDate, eDate);

                // 기존 데이터 조회 소요 시간 계산
                TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;
                _logger.LogPerformance("기존 데이터 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");

                // Database와 비교하여 Database에 없는것 입력
                List<FlowData> addDatas = resultList.Where(current =>
                    !flowData_DB.Any(db => db.ymd == current.ymd && db.obscd == current.obscd)
                ).ToList();

                _logger.Info($"저장할 새로운 데이터: {addDatas.Count}건", "Database");

                if (addDatas.Count > 0)
                {
                    // DB 삽입 시작 시간 기록
                    DateTime dbInsertStart = DateTime.Now;

                    // Bulk Insert 실행
                    bool success = NpgSQLService.BulkInsert_WAMISFlowDatas(addDatas);

                    // DB 삽입 소요 시간 계산
                    TimeSpan dbInsertDuration = DateTime.Now - dbInsertStart;
                    _logger.LogPerformance("데이터베이스 삽입", (long)dbInsertDuration.TotalMilliseconds, "Database");

                    if (success)
                    {
                        _logger.Info($"데이터베이스 저장 성공: [{obsCD}] 관측소 {sDate.Substring(0, 4)}년 {addDatas.Count}건", "Database");
                    }
                    else
                    {
                        _logger.Error($"데이터베이스 저장 실패: [{obsCD}] 관측소 {addDatas.Count}건", "Database");
                    }
                }
                else
                {
                    _logger.Info("저장할 새로운 데이터가 없습니다.", "Database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "결과 처리 중 오류 발생", LogLevel.Error, "Database");
            }
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
