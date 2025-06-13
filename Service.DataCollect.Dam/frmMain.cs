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
        private LogManager _logger; // 로그 관리자 인스턴스
        #endregion

        #region [Threads]
        private Thread thOpenAPI_WAMIS_mnhrdata { get; set; }
        private Thread thOpenAPI_WAMIS_mnhrdata_Period { get; set; }
        private Thread thOpenAPI_WAMIS_mnhrdata_Result { get; set; }
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
            _logger.Initialize(listStatus, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"), "Dam");
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
            _global.startDate = new DateTime(Config.StartDate, 1, 1);
            _global.endDate = new DateTime(Config.EndDate, 12, 31);

            _logger.Debug($"실시간 사용: {_global.RealTimeUse}, 기간 사용: {_global.PeriodUse}", "Config");
            _logger.Debug($"시작일: {_global.startDate:yyyy-MM-dd}, 종료일: {_global.endDate:yyyy-MM-dd}", "Config");
        }

        private void InitializeSites()
        {
            _logger.Info("댐 사이트 정보 초기화 중...", "Initialize");

            WamisAPIController apiController = new WamisAPIController();
            DataTable dt = new DataTable();
            dt = apiController.GetDamSiteData("mn_dammain");
            List<DamSiteInformation> listDam = new List<DamSiteInformation>();
            listDam = BizCommon.ConvertDataTableToList<DamSiteInformation>(dt);
            _global.listDams = listDam;

            _logger.Info($"{listDam.Count}개의 댐 정보 로드 완료", "Initialize");
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

                _logger.Debug("데이터베이스 연결 문자열 생성 완료", "Database");

                using (NpgsqlConnection NpgSQLconn = new NpgsqlConnection(strConn))
                {
                    NpgSQLconn.Open();
                    _logger.Debug("데이터베이스 연결 열기 성공", "Database");
                    NpgSQLconn.Close();
                    _logger.Debug("데이터베이스 연결 닫기 성공", "Database");
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
                _logger.Info("서비스 시작 중...", "Service");
                _shouldStop = false;

                // WAMIS 실시간 데이터 수집 스레드
                if (_global.RealTimeUse == true && _global.PeriodUse != true)
                {
                    _logger.Info("실시간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_WAMIS_mnhrdata = new Thread(OpenAPI_Wamis_mndtdata_AutoCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_mnhrdata.Start();
                }

                // WAMIS 기간 데이터 수집 스레드
                if (_global.PeriodUse == true)
                {
                    _logger.Info("기간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_WAMIS_mnhrdata_Period = new Thread(OpenAPI_WAMIS_mnhrdata_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_mnhrdata_Period.Start();
                }

                // WAMIS 결과 처리 스레드
                _logger.Info("결과 처리 스레드 시작", "Thread");
                thOpenAPI_WAMIS_mnhrdata_Result = new Thread(OpenAPI_WAMIS_mnhrdata_ResultCaller)
                {
                    IsBackground = true
                };
                thOpenAPI_WAMIS_mnhrdata_Result.Start();

                isServiceRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 시작 중 오류 발생", LogLevel.Error, "Service");
                return false;
            }
        }

        private void OpenAPI_Wamis_mndtdata_AutoCaller()
        {
            // 설정된 시간 간격으로 호출 (초 단위)
            int nTimeGap = 1000 * Config.OpenAPI_Wamis_mndtdata_Second;
            _logger.Info($"실시간 데이터 수집 간격: {Config.OpenAPI_Wamis_mndtdata_Second}초", "AutoCaller");

            deleOpenAPI_Wamis_mndtdata_Caller deleService_Wamis_mndtdata =
                new deleOpenAPI_Wamis_mndtdata_Caller(this.OpenAPI_Wamis_mndtdata_Service);

            while (!_shouldStop) // 종료 플래그 확인
            {
                _logger.Debug("실시간 데이터 수집 호출 시작", "AutoCaller");
                IAsyncResult ar = deleService_Wamis_mndtdata.BeginInvoke(null, null);
                // nTimeGap 만큼 Sleep
                Thread.Sleep(nTimeGap);
            }

            _logger.Info("실시간 데이터 수집 스레드 종료", "AutoCaller");
        }

        private void OpenAPI_Wamis_mndtdata_Service()
        {
            try
            {
                _logger.Info("댐 수문정보 실시간 데이터 수집 모듈 시작", "Service");

                // DB에서 최종 데이터 일자 조회
                DateTime startDate;
                DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_WAMIS_mnhrdata();

                _logger.Info($"최종 데이터 일자: {lastDate:yyyy-MM-dd HH:mm}", "Service");

                // 최종 데이터가 없거나 오류 발생 시 기본값 설정 (30일 전)
                if (lastDate == DateTime.MinValue)
                {
                    startDate = DateTime.Now.AddDays(-30);
                    _logger.Warning("최종 데이터가 없어 기본값(30일 전)으로 설정합니다.", "Service");
                }
                else
                {
                    // 마지막 데이터 다음날부터 시작
                    startDate = lastDate.AddDays(0);
                    // 마지막 데이터 다음날이 오늘보다 늦은 경우 작업 취소
                    if (startDate > DateTime.Now)
                    {
                        string message = $"최종 데이터({lastDate:yyyy-MM-dd HH:mm})가 오늘({DateTime.Now:yyyy-MM-dd HH:mm})데이터 입니다. 데이터 수집을 건너뜁니다.";
                        _logger.Warning(message, "Service");
                        return; // 메서드 종료
                    }
                }

                // 종료일은 오늘 날짜로 설정
                DateTime endDate = DateTime.Now;

                _logger.Info($"데이터 수집 기간: {startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}", "Service");

                // WAMIS API 기본 URL 설정
                string serviceURL = "http://www.wamis.go.kr:8080/wamis/openapi/wkd/mn_hrdata";
                string authKey = "b4568bbc61dabc1ce232c94d538f9f7d45229c1620";

                int totalProcessed = 0;
                int totalSuccess = 0;

                // 모든 댐에 대해 데이터 수집
                foreach (DamSiteInformation dam in _global.listDams)
                {
                    if (_shouldStop) break; // 종료 플래그 확인

                    _logger.Info($"[{dam.damnm}] 댐 데이터 수집 시작 ({startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd})", "API");

                    // API 요청 파라미터 설정 - 날짜 형식 명확하게 지정
                    string parameters = string.Format("?damcd={0}&startdt={1}&enddt={2}&authKey={3}",
                        dam.damcd,
                        startDate.ToString("yyyyMMdd"),
                        endDate.ToString("yyyyMMdd"),
                        authKey);

                    // 전체 URI 구성
                    Uri uri = new Uri(serviceURL + parameters);
                    _logger.Debug($"API 요청 URL: {uri}", "API");

                    // API 호출 시작 시간 기록
                    DateTime apiCallStart = DateTime.Now;

                    // WAMIS API 호출하여 댐 수문 데이터 가져오기
                    List<DamHRData> damDatas = WAMIS_Controller.GetDamHrData(dam.damcd, startDate, endDate);

                    // API 호출 소요 시간 계산
                    TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                    _logger.LogPerformance($"[{dam.damnm}] API 호출", (long)apiCallDuration.TotalMilliseconds);

                    if (damDatas != null && damDatas.Count > 0)
                    {
                        _logger.Info($"수집된 {dam.damnm} 댐 데이터: {damDatas.Count}개", "API");
                        EnqueueOpenAPIWAMISDamHrResult(damDatas);
                        totalSuccess++;
                        totalProcessed += damDatas.Count;
                    }
                    else
                    {
                        _logger.Warning($"{dam.damnm} 댐 데이터 없음", "API");
                    }

                    // 연속 API 호출 시 서버 부하 방지를 위한 대기
                    Thread.Sleep(1000);
                }

                _logger.Info($"실시간 데이터 수집 완료: 총 {_global.listDams.Count}개 댐 중 {totalSuccess}개 성공, {totalProcessed}건 데이터 처리", "Service");
                _logger.Info("댐 수문정보 실시간 데이터 수집 모듈 종료", "Service");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "댐 수문정보 실시간 데이터 수집 모듈 오류", LogLevel.Error, "Service");
            }
        }

        private void OpenAPI_WAMIS_mnhrdata_PeriodCaller()
        {
            try
            {
                _logger.Info("댐 수문정보 기간 조회 모듈 시작", "PeriodCaller");

                DateTime stDate = _global.startDate;
                DateTime edDate = _global.endDate;

                _logger.Info($"설정된 기간: {stDate:yyyy-MM-dd} ~ {edDate:yyyy-MM-dd}", "PeriodCaller");

                int totalProcessed = 0;
                int totalSuccess = 0;

                foreach (DamSiteInformation dam in _global.listDams)
                {
                    if (_shouldStop) break; // 종료 플래그 확인

                    _logger.Info($"[{dam.damnm}] 댐 기간 데이터 수집 시작", "PeriodCaller");

                    // 6개월 단위로 데이터 수집 (API 요청 제한 고려)
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

                        _logger.Debug($"[{dam.damnm}] 조회 기간: {Search_stDate:yyyy-MM-dd} ~ {Search_edDate:yyyy-MM-dd}", "PeriodCaller");

                        // API 호출 시작 시간 기록
                        DateTime apiCallStart = DateTime.Now;

                        // WAMIS API 호출하여 댐 수문 데이터 가져오기
                        List<DamHRData> damDatas = WAMIS_Controller.GetDamHrData(dam.damcd, Search_stDate, Search_edDate);

                        // API 호출 소요 시간 계산
                        TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                        _logger.LogPerformance($"[{dam.damnm}] API 호출", (long)apiCallDuration.TotalMilliseconds);

                        if (damDatas != null && damDatas.Count > 0)
                        {
                            _logger.Info($"기간별 수집된 {dam.damnm} 댐 데이터: {damDatas.Count}개 ({Search_stDate:yyyy-MM-dd} ~ {Search_edDate:yyyy-MM-dd})", "PeriodCaller");
                            EnqueueOpenAPIWAMISDamHrResult(damDatas);
                            totalSuccess++;
                            totalProcessed += damDatas.Count;
                        }
                        else
                        {
                            _logger.Warning($"[{dam.damnm}] 댐 데이터 없음 ({Search_stDate:yyyy-MM-dd} ~ {Search_edDate:yyyy-MM-dd})", "PeriodCaller");
                        }

                        // 연속 API 호출 시 서버 부하 방지를 위한 대기
                        Thread.Sleep(100);
                    }
                }

                _logger.Info($"기간 데이터 수집 완료: 총 {_global.listDams.Count}개 댐 중 {totalSuccess}개 성공, {totalProcessed}건 데이터 처리", "PeriodCaller");
                _logger.Info("댐 수문정보 기간 조회 모듈 종료", "PeriodCaller");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "댐 수문정보 기간 조회 모듈 오류", LogLevel.Error, "PeriodCaller");
            }
        }

        private void EnqueueOpenAPIWAMISDamHrResult(List<DamHRData> damDatas)
        {
            try
            {
                _logger.Debug($"결과 큐에 {damDatas.Count}개 데이터 추가 중", "Queue");

                // 결과 파일명 Queue에 넣기
                lock (locker) OpenAPI_WAMIS_mnhrdata_ResultQueue.Enqueue(damDatas);

                // 결과 처리하도록 이벤트 발생
                eventWaitHandle.Set();

                _logger.Debug("결과 큐에 데이터 추가 완료", "Queue");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "결과 큐 데이터 추가 중 오류 발생", LogLevel.Error, "Queue");
            }
        }

        private void OpenAPI_WAMIS_mnhrdata_ResultCaller()
        {
            _logger.Info("댐 수문정보 결과 처리 모듈 시작", "ResultCaller");

            while (!_shouldStop) // 종료 플래그 확인
            {
                List<DamHRData> resultList = new List<DamHRData>();
                bool hasData = false;

                lock (locker)
                {
                    if (OpenAPI_WAMIS_mnhrdata_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_WAMIS_mnhrdata_ResultQueue.Dequeue();
                        hasData = true;
                        _logger.Debug($"결과 큐에서 {resultList.Count}개 데이터 가져옴", "ResultCaller");
                    }
                }

                if (hasData && resultList.Count > 0)
                {
                    // 결과 저장 로직
                    OpenAPIWAMISDamHRResultInsertProcess(resultList);
                }
                else
                {
                    // 파일명 없을때 Wait Signal (최대 100ms 대기 후 종료 플래그 확인)
                    eventWaitHandle.WaitOne(100);
                }
            }

            _logger.Info("댐 수문정보 결과 처리 모듈 종료", "ResultCaller");
        }

        private void OpenAPIWAMISDamHRResultInsertProcess(List<DamHRData> resultList)
        {
            try
            {
                if (resultList == null || resultList.Count == 0)
                {
                    _logger.Warning("처리할 데이터가 없습니다.", "Database");
                    return;
                }

                string sDate = resultList.First().obsdh;
                string eDate = resultList.Last().obsdh;
                string damcd = resultList.First().damcd;

                _logger.Info($"데이터베이스 처리 시작: 댐 코드 {damcd}, 기간 {sDate} ~ {eDate}", "Database");

                // 데이터베이스 조회 시작 시간 기록
                DateTime dbQueryStart = DateTime.Now;

                // 기존 데이터 조회
                List<DamHRData> DamHrDatas_DB = NpgSQLService.GetDailyDatasFromOpenAPIWAMISDamHrData(damcd, sDate, eDate);

                // 데이터베이스 조회 소요 시간 계산
                TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;
                _logger.LogPerformance("데이터베이스 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");

                // Database와 비교하여 Database에 없는것 입력
                List<DamHRData> addDatas = resultList.Where(current =>
                    !DamHrDatas_DB.Any(db => db.obsdh == current.obsdh && db.damcd == current.damcd)).ToList();

                _logger.Info($"신규 데이터 {addDatas.Count}개 발견 (전체 {resultList.Count}개 중)", "Database");

                if (addDatas.Count > 0)
                {
                    // 데이터베이스 삽입 시작 시간 기록
                    DateTime dbInsertStart = DateTime.Now;

                    // Bulk Insert 실행
                    bool success = NpgSQLService.BulkInsert_WAMISDamHrDatas(addDatas);

                    // 데이터베이스 삽입 소요 시간 계산
                    TimeSpan dbInsertDuration = DateTime.Now - dbInsertStart;
                    _logger.LogPerformance("데이터베이스 삽입", (long)dbInsertDuration.TotalMilliseconds, "Database");

                    if (success)
                    {
                        _logger.Info($"데이터베이스 삽입 성공: {damcd} 댐, {sDate.Substring(0, 4)}년, {addDatas.Count}개 데이터", "Database");
                    }
                    else
                    {
                        _logger.Error($"데이터베이스 삽입 실패: {damcd} 댐, {addDatas.Count}개 데이터", "Database");
                    }
                }
                else
                {
                    _logger.Info("모든 데이터가 이미 데이터베이스에 존재합니다.", "Database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "데이터베이스 처리 중 오류 발생", LogLevel.Error, "Database");
            }
        }

        private void ServiceStop()
        {
            try
            {
                _logger.Info("서비스 중지 요청", "Service");

                // 종료 플래그 설정
                _shouldStop = true;

                // 대기 중인 스레드 깨우기
                eventWaitHandle.Set();

                // 각 스레드가 종료될 때까지 대기
                if (thOpenAPI_WAMIS_mnhrdata != null && thOpenAPI_WAMIS_mnhrdata.IsAlive)
                {
                    _logger.Debug("실시간 데이터 수집 스레드 종료 대기 중", "Service");
                    thOpenAPI_WAMIS_mnhrdata.Join(100);
                }

                if (thOpenAPI_WAMIS_mnhrdata_Period != null && thOpenAPI_WAMIS_mnhrdata_Period.IsAlive)
                {
                    _logger.Debug("기간 데이터 수집 스레드 종료 대기 중", "Service");
                    thOpenAPI_WAMIS_mnhrdata_Period.Join(100);
                }

                if (thOpenAPI_WAMIS_mnhrdata_Result != null && thOpenAPI_WAMIS_mnhrdata_Result.IsAlive)
                {
                    _logger.Debug("결과 처리 스레드 종료 대기 중", "Service");
                    thOpenAPI_WAMIS_mnhrdata_Result.Join(100);
                }

                isServiceRunning = false; // 서비스 실행 상태 해제
                _logger.Info("서비스가 중지되었습니다.", "Service");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 중지 중 오류 발생", LogLevel.Error, "Service");
            }
        }

        private void periodSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.Debug("기간 설정 메뉴 클릭", "UI");
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
                _logger.LogException(ex, "폼 표시 중 오류 발생", LogLevel.Error, "UI");
                return DialogResult.None;
            }
        }
        #endregion
    }
    #endregion
}