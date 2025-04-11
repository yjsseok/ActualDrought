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

namespace Service.DataCollect.ASOS
{
    public partial class frmMain : Form
    {
        #region [Thread 변수]
        delegate void deleKMAASOS_AutoCaller();
        private Thread thOpenAPI_KMA_ASOS { get; set; }
        private Thread thOpenAPI_KMA_ASOS_Period { get; set; }
        private Thread thOpenAPI_KMA_ASOS_Result { get; set; }
        #endregion

        #region [WorkerThread]
        private EventWaitHandle eventWaitHandle = new AutoResetEvent(false);
        private readonly object locker = new object();
        private Queue<string> OpenAPI_KMA_ASOS_ResultQueue = new Queue<string>();
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
            _logger.Initialize(listStatus, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"), "ASOS");
            _logger.Info("애플리케이션 시작", "System");

            InitializeLogNBuild();
            InitializeVariables();

            if (InitializeDatabase() == true)
            {
                _logger.Info("시스템 초기화 완료", "Initialize");
            }
            else
            {
                _logger.Error("데이터베이스 연결 실패", "Database");
                MessageBox.Show("데이터베이스 연결에 실패했습니다. 설정을 확인해주세요.", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 리스트박스 상태 확인
            CheckListBoxEmpty();
        }

        private void InitializeLogNBuild()
        {
            //Log설정
            GMLogManager.ConfigureLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"));

            string version = string.Format("V{0}.{1}.{2}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);

            this.Text += " " + version;
            _logger.Info($"버전: {version}", "System");
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

        private bool InitializeDatabase()
        {
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

                _logger.Debug("데이터베이스 연결 시도 중...", "Database");

                using (NpgsqlConnection NpgSQLconn = new NpgsqlConnection(strConn))
                {
                    NpgSQLconn.Open();
                    _logger.Debug("데이터베이스 연결 성공", "Database");
                    NpgSQLconn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "데이터베이스 연결 실패", LogLevel.Error, "Database");
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
                #region [기상청 자료수집 구동]
                if (_global.RealTimeUse == true)
                {
                    _logger.Info("실시간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_KMA_ASOS = new Thread(OpenAPI_KMA_ASOS_AutoCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_KMA_ASOS.Start();
                }
                #endregion

                #region [기상청 자료수집 기간]
                if (_global.PeriodUse == true)
                {
                    _logger.Info("기간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_KMA_ASOS_Period = new Thread(OpenAPI_KMA_ASOS_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_KMA_ASOS_Period.Start();
                }
                #endregion

                #region [기상청 결과처리]
                _logger.Info("결과 처리 스레드 시작", "Thread");
                thOpenAPI_KMA_ASOS_Result = new Thread(OpenAPI_KMA_ASOS_ResultCaller)
                {
                    IsBackground = true
                };
                thOpenAPI_KMA_ASOS_Result.Start();
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
                // 종료 플래그 설정
                _shouldStop = true;

                // 대기 중인 스레드 깨우기
                eventWaitHandle.Set();

                // 모든 스레드 종료
                if (thOpenAPI_KMA_ASOS != null && thOpenAPI_KMA_ASOS.IsAlive)
                {
                    _logger.Debug("실시간 데이터 수집 스레드 종료 중...", "Thread");
                    thOpenAPI_KMA_ASOS.Abort();
                }

                if (thOpenAPI_KMA_ASOS_Period != null && thOpenAPI_KMA_ASOS_Period.IsAlive)
                {
                    _logger.Debug("기간 데이터 수집 스레드 종료 중...", "Thread");
                    thOpenAPI_KMA_ASOS_Period.Abort();
                }

                if (thOpenAPI_KMA_ASOS_Result != null && thOpenAPI_KMA_ASOS_Result.IsAlive)
                {
                    _logger.Debug("결과 처리 스레드 종료 중...", "Thread");
                    thOpenAPI_KMA_ASOS_Result.Abort();
                }

                isServiceRunning = false; // 서비스 실행 상태 해제
                _logger.Info("모든 서비스가 중지되었습니다.", "Service");
                MessageBox.Show("ASOS 서비스가 성공적으로 중지되었습니다.", "서비스 상태", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 중지 중 오류 발생", LogLevel.Error, "Service");
                MessageBox.Show($"서비스 중지 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenAPI_KMA_ASOS_ResultCaller()
        {
            _logger.Info("ASOS 결과 처리 모듈 시작", "ResultCaller");

            while (!_shouldStop)
            {
                string resultPath = string.Empty;

                lock (locker)
                {
                    if (OpenAPI_KMA_ASOS_ResultQueue.Count > 0)
                    {
                        resultPath = OpenAPI_KMA_ASOS_ResultQueue.Dequeue();
                        // If file name is null then stop worker thread
                        if (resultPath == string.Empty) return;
                    }
                }

                if (resultPath != string.Empty)
                {
                    // 결과 저장 로직
                    _logger.Info($"파일 처리 중: {resultPath}", "ResultCaller");
                    OpenAPIKMAASOSResultInsertProcess(resultPath);
                }
                else
                {
                    // 파일명 없을때 Wait Signal
                    eventWaitHandle.WaitOne(100);
                }
            }

            _logger.Info("ASOS 결과 처리 모듈 종료", "ResultCaller");
        }

        private void OpenAPIKMAASOSResultInsertProcess(string resultPath)
        {
            try
            {
                _logger.Debug($"파일 처리 시작: {resultPath}", "ResultProcess");
                DateTime processStart = DateTime.Now;

                List<rcvKMAASOSData> listKMAASOS = new List<rcvKMAASOSData>();
                listKMAASOS = KMA_Controller.FiletoList_KMAASOS(resultPath);

                if (listKMAASOS == null || listKMAASOS.Count == 0)
                {
                    _logger.Warning($"파일에서 데이터를 찾을 수 없습니다: {resultPath}", "ResultProcess");
                    return;
                }

                _logger.Info($"파일에서 {listKMAASOS.Count}개 데이터 로드 완료", "ResultProcess");

                string filePathWithoutExt = Path.GetFileNameWithoutExtension(resultPath);
                //파일명에서 정보추출
                string[] splitedstring = filePathWithoutExt.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                int stdID = int.Parse(splitedstring[1].Trim());
                DateTime sDate = BizCommon.StringtoDateTimeStart(splitedstring[2].Trim());
                DateTime eDate = BizCommon.StringtoDateTimeEnd(splitedstring[2].Trim());

                _logger.Debug($"파일 정보: 관측소ID={stdID}, 시작일={sDate:yyyy-MM-dd}, 종료일={eDate:yyyy-MM-dd}", "ResultProcess");

                //입력 동일기간의 데이터를 Database에서 조회
                DateTime dbQueryStart = DateTime.Now;
                List<rcvKMAASOSData> listKMAASOS_Database = new List<rcvKMAASOSData>();
                listKMAASOS_Database = NpgSQLService.GetDailyDatas_FromOpenAPI_KMAASOS(stdID, sDate, eDate);
                TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;

                _logger.LogPerformance("기존 데이터 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");
                _logger.Debug($"기존 데이터 조회 결과: {listKMAASOS_Database.Count}건", "ResultProcess");

                //Database와 비교하여 Database에 없는것 입력
                List<rcvKMAASOSData> addDatas = listKMAASOS.Where(current =>
                    !listKMAASOS_Database.Any(db => db.TM == current.TM && db.STN == current.STN)).ToList();

                _logger.Info($"새로 추가할 데이터: {addDatas.Count}건", "ResultProcess");

                if (addDatas.Count > 0)
                {
                    //Bulk Insert 실행
                    _logger.Info($"데이터베이스에 {addDatas.Count}건 저장 중...", "Database");

                    DateTime dbInsertStart = DateTime.Now;
                    bool result = NpgSQLService.BulkInsert_KMAASOSDatas(addDatas);
                    TimeSpan dbInsertDuration = DateTime.Now - dbInsertStart;

                    _logger.LogPerformance("데이터베이스 삽입", (long)dbInsertDuration.TotalMilliseconds, "Database");

                    if (result)
                    {
                        _logger.Info($"데이터베이스 저장 성공: 관측소ID={stdID}, {addDatas.Count}건", "Database");
                    }
                    else
                    {
                        _logger.Error($"데이터베이스 저장 실패: 관측소ID={stdID}, {addDatas.Count}건", "Database");
                    }
                }
                else
                {
                    _logger.Info("저장할 새로운 데이터가 없습니다.", "ResultProcess");
                }

                TimeSpan totalDuration = DateTime.Now - processStart;
                _logger.LogPerformance("전체 처리 시간", (long)totalDuration.TotalMilliseconds, "ResultProcess");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "결과 처리 중 오류 발생", LogLevel.Error, "ResultProcess");
            }
        }

        private void OpenAPI_KMA_ASOS_PeriodCaller()
        {
            _logger.Info("기간별 ASOS 데이터 수집 시작", "PeriodCaller");

            try
            {
                //기간데이터
                DateTime reqStartDate = new DateTime(
                    _global.startDate.Year,
                    _global.startDate.Month,
                    _global.startDate.Day,
                    0, 0, 0);

                DateTime reqEndDate = new DateTime(
                    _global.endDate.Year,
                    _global.endDate.Month,
                    _global.endDate.Day,
                    23, 59, 0);

                // 현재 날짜보다 이후의 날짜는 처리하지 않음
                if (reqEndDate > DateTime.Today)
                {
                    reqEndDate = DateTime.Today;
                    _logger.Warning("종료일이 오늘 이후로 설정되어 있어 오늘까지만 처리합니다.", "PeriodCaller");
                }

                int totalDays = (reqEndDate - reqStartDate).Days + 1;
                int processedDays = 0;
                int dataFoundDays = 0;

                _logger.Info($"기간 데이터 수집: {reqStartDate:yyyy-MM-dd} ~ {reqEndDate:yyyy-MM-dd}, 총 {totalDays}일", "PeriodCaller");

                for (DateTime dt = reqStartDate; dt <= reqEndDate; dt = dt.AddDays(1))
                {
                    DateTime stDate = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                    DateTime edDate = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 00);

                    //기관용 주소
                    string serviceURL = "https://apihub-pub.kma.go.kr/api/typ01/url/kma_sfcdd.php?";

                    //시작시간
                    string tm = dt.ToString("yyyyMMdd");

                    //지점코드
                    string stn = "0";

                    //공백 : 0 콤마 : 1
                    string disp = "1";

                    //1 : Head에 도움말있음 , 0 : Head에 도움말 없음
                    string help = "0";

                    string authKey = "40cfe353913cd680317889498823f9214c0d7a09e09583a5b09291467c37af3414233051807b848caa2d0162742948c28969c222eb3dcdfc061abea91ac9d60a";

                    _logger.Info($"요청 중... 날짜: {tm}", "PeriodCaller");

                    // API 호출 시작 시간 기록
                    DateTime apiCallStart = DateTime.Now;

                    Uri uri = new Uri(string.Format("{0}tm={1}&stn={2}&disp={3}&help={4}&authKey={5}",
                        serviceURL, tm, stn, disp, help, authKey));

                    string filePath = KMA_Controller.ExecuteDownloadResponse(uri, tm, stn);

                    // API 호출 소요 시간 계산
                    TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                    _logger.LogPerformance($"API 호출 (날짜: {tm})", (long)apiCallDuration.TotalMilliseconds, "API");

                    if (filePath != string.Empty)
                    {
                        _logger.Info($"파일 생성 완료: {filePath}", "PeriodCaller");
                        EnqueueOpenAPIKMAASOSResult(filePath);
                        dataFoundDays++;
                    }
                    else
                    {
                        _logger.Warning($"{tm} 날짜의 데이터가 없습니다", "PeriodCaller");
                    }

                    processedDays++;

                    // 진행 상황 로깅
                    if (processedDays % 10 == 0 || processedDays == totalDays)
                    {
                        _logger.Info($"진행 상황: {processedDays}/{totalDays}일 처리 완료 ({(int)(processedDays * 100.0 / totalDays)}%)", "PeriodCaller");
                    }
                }

                string completionMessage = $"기간별 데이터 수집 완료: 총 {totalDays}일 중 {dataFoundDays}일의 데이터를 찾았습니다.";
                _logger.Info(completionMessage, "PeriodCaller");

                // 작업 완료 메시지 표시
                this.Invoke(new Action(() =>
                    MessageBox.Show(completionMessage, "기간별 수집 완료", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "기간별 ASOS 데이터 수집 중 오류 발생", LogLevel.Error, "PeriodCaller");

                // 오류 발생 시 메시지 박스로 알림
                this.Invoke(new Action(() =>
                    MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ));
            }
        }

        private void OpenAPI_KMA_ASOS_AutoCaller()
        {
            _logger.Info("실시간 ASOS 데이터 수집 스레드 시작", "AutoCaller");

            //ASOS일데이터에 맞게 호출시간 변경
            int nTimeGap = 1000 * Config.KMA_ASOS_Auto_Caller_Second;
            _logger.Debug($"실시간 데이터 수집 간격: {Config.KMA_ASOS_Auto_Caller_Second}초", "AutoCaller");

            deleKMAASOS_AutoCaller deleKMAASOSMethod = new deleKMAASOS_AutoCaller(this.OpenAPI_KMA_ASOS);

            while (!_shouldStop)
            {
                _logger.Debug("실시간 데이터 수집 호출 시작", "AutoCaller");
                IAsyncResult ar = deleKMAASOSMethod.BeginInvoke(null, null);
                Thread.Sleep(nTimeGap);
            }

            _logger.Info("실시간 ASOS 데이터 수집 스레드 종료", "AutoCaller");
        }

        private void OpenAPI_KMA_ASOS()
        {
            _logger.Info("OpenAPI KMA ASOS Service 시작됨", "Service");

            try
            {
                #region [데이터 수집 Layer]
                // 관측소 조회
                List<KMASiteInformation> listAWS = new List<KMASiteInformation>();

                DateTime dbQueryStart = DateTime.Now;
                listAWS = NpgSQLService.GetSites_FromOpenAPI_KMAASOS();
                TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;

                _logger.LogPerformance("관측소 정보 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");

                //관측소별 데이터요청 (파일로 다운)
                if (listAWS != null)
                {
                    _logger.Info($"관측소 정보 {listAWS.Count}개 로드 완료", "Service");

                    // 데이터베이스에서 tm의 최종 일자를 조회
                    DateTime lastDateQueryStart = DateTime.Now;
                    DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_KMAASOS();
                    TimeSpan lastDateQueryDuration = DateTime.Now - lastDateQueryStart;

                    _logger.LogPerformance("최종 데이터 일자 조회", (long)lastDateQueryDuration.TotalMilliseconds, "Database");
                    _logger.Info($"최종 데이터 일자: {lastDate:yyyy-MM-dd}", "Service");

                    DateTime today = DateTime.Today;

                    // 최종 데이터가 오늘 날짜보다 이후인 경우 작업 취소
                    if (lastDate.Date >= today.Date)
                    {
                        string message = $"최종 데이터({lastDate:yyyy-MM-dd})가 오늘자입니다. 데이터 수집을 건너뜁니다.";
                        _logger.Warning(message, "Service");

                        // 메시지 박스로 알림 표시
                        this.Invoke(new Action(() =>
                            MessageBox.Show(message, "데이터 수집 알림", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ));

                        return; // 메서드 종료
                    }

                    // 최종 일자부터 오늘까지의 데이터를 요청
                    int totalDays = (today - lastDate.AddDays(1)).Days + 1;
                    int processedDays = 0;
                    int dataFoundDays = 0;

                    _logger.Info($"데이터 수집 시작: {lastDate.AddDays(1):yyyy-MM-dd} ~ {today:yyyy-MM-dd}, 총 {totalDays}일", "Service");

                    for (DateTime date = lastDate.AddDays(1); date <= today; date = date.AddDays(1))
                    {
                        string tm = date.ToString("yyyyMMdd");
                        string tm2 = today.ToString("yyyyMMdd");

                        _logger.Info($"{tm} 날짜의 데이터 수집 시작", "Service");

                        string serviceURL = "https://apihub-pub.kma.go.kr/api/typ01/url/kma_sfcdd3.php?";
                        string stn = "0";
                        string disp = "1";
                        string help = "0";
                        string authKey = "40cfe353913cd680317889498823f9214c0d7a09e09583a5b09291467c37af3414233051807b848caa2d0162742948c28969c222eb3dcdfc061abea91ac9d60a";

                        // API 호출 시작 시간 기록
                        DateTime apiCallStart = DateTime.Now;

                        Uri uri = new Uri(string.Format("{0}tm={1}&tm2={2}&stn={3}&disp={4}&help={5}&authKey={6}",
                            serviceURL, tm, tm2, stn, disp, help, authKey));

                        string filePath = KMA_Controller.ExecuteDownloadResponse(uri, tm, stn);

                        // API 호출 소요 시간 계산
                        TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                        _logger.LogPerformance($"API 호출 (날짜: {tm})", (long)apiCallDuration.TotalMilliseconds, "API");

                        #region [데이터 입력 Layer]
                        //데이터 들어온거 확인
                        if (filePath != string.Empty)
                        {
                            _logger.Info($"{tm} 날짜의 데이터 수집 완료", "Service");
                            EnqueueOpenAPIKMAASOSResult(filePath);
                            dataFoundDays++;
                        }
                        else
                        {
                            _logger.Warning($"{tm} 날짜의 데이터가 없습니다", "Service");
                        }
                        #endregion

                        processedDays++;

                        // 진행 상황 로깅
                        if (processedDays % 5 == 0 || processedDays == totalDays)
                        {
                            _logger.Info($"진행 상황: {processedDays}/{totalDays}일 처리 완료 ({(int)(processedDays * 100.0 / totalDays)}%)", "Service");
                        }
                    }

                    _logger.Info($"실시간 데이터 수집 완료: 총 {totalDays}일 중 {dataFoundDays}일의 데이터를 찾았습니다.", "Service");
                }
                else
                {
                    _logger.Error("관측소 정보를 가져오는데 실패했습니다.", "Service");
                }
                #endregion

                _logger.Info("OpenAPI KMA ASOS Service 완료됨", "Service");

                // 작업 완료 메시지 표시
                this.Invoke(new Action(() =>
                    MessageBox.Show("ASOS 데이터 수집이 완료되었습니다.", "작업 완료", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "OpenAPI KMA ASOS Service 오류 발생", LogLevel.Error, "Service");

                // 오류 발생 시 메시지 박스로 알림
                this.Invoke(new Action(() =>
                    MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ));
            }
        }

        private void EnqueueOpenAPIKMAASOSResult(string filePath)
        {
            try
            {
                _logger.Debug($"결과 큐에 파일 추가: {filePath}", "Queue");

                //결과 파일명 Queue에 넣기
                lock (locker)
                {
                    OpenAPI_KMA_ASOS_ResultQueue.Enqueue(filePath);
                    _logger.Debug($"현재 큐 크기: {OpenAPI_KMA_ASOS_ResultQueue.Count}", "Queue");
                }

                //결과 처리하도록 이벤트 발생
                eventWaitHandle.Set();
                _logger.Debug("결과 처리 이벤트 발생", "Queue");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "결과 큐 추가 중 오류 발생", LogLevel.Error, "Queue");
            }
        }

        // 리스트박스가 비어있는지 확인하는 메서드
        private void CheckListBoxEmpty()
        {
            if (listStatus.Items.Count == 0)
            {
                listStatus.Items.Add("표시할 데이터가 없습니다.");
                _logger.Debug("리스트박스가 비어 있어 안내 메시지 추가", "UI");
            }
        }
        #endregion

        #region [메뉴]
        private void periodSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.Debug("기간 설정 메뉴 클릭", "UI");
            ShowForm(typeof(frmConfig), true, true);
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.Debug("설정 메뉴 클릭", "UI");
            // 설정 관련 코드 구현
        }
        #endregion

        #region [FormShow 관련 함수들]
       
        private DialogResult ShowForm(Type type, bool isPopup, bool isModal)
        {
            try
            {
                if (type == null)
                {
                    _logger.Error("폼 타입이 null입니다", "UI");
                    throw new ArgumentException("type is null");
                }

                // 팝업이 아니면 기존에 열려있는 폼 닫기
                if (!isPopup)
                {
                    foreach (var frm in this.MdiChildren)
                    {
                        if (frm.GetType() == type)
                        {
                            _logger.Debug($"기존 열린 폼 활성화: {type.Name}", "UI");
                            frm.Activate();
                            return DialogResult.None;
                        }
                    }
                }

                _logger.Debug($"새 폼 생성: {type.Name}, 팝업: {isPopup}, 모달: {isModal}", "UI");
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

    }
}
#endregion

