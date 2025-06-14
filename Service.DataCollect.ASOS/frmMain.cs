﻿using Npgsql;
using OpenAPI.Controls;
using OpenAPI.DataServices;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

            if (InitializeDatabase() == true)
            {
                WriteStatus("시스템 초기화 완료");
            }
            else
            {
                WriteStatus("데이터베이스 연결 실패");
                MessageBox.Show("데이터베이스 연결에 실패했습니다. 설정을 확인해주세요.", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 리스트박스 상태 확인
            CheckListBoxEmpty();
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
                #region [기상청 자료수집 구동]
                if (_global.RealTimeUse == true)
                {
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
                    thOpenAPI_KMA_ASOS_Period = new Thread(OpenAPI_KMA_ASOS_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_KMA_ASOS_Period.Start();
                }
                #endregion

                #region [기상청 결과처리]
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
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

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

                Thread.Sleep(5000);



                // 모든 스레드 종료
                if (thOpenAPI_KMA_ASOS != null && thOpenAPI_KMA_ASOS.IsAlive)
                {
                    thOpenAPI_KMA_ASOS.Abort();
                }

                if (thOpenAPI_KMA_ASOS_Period != null && thOpenAPI_KMA_ASOS_Period.IsAlive)
                {
                    thOpenAPI_KMA_ASOS_Period.Abort();
                }

                if (thOpenAPI_KMA_ASOS_Result != null && thOpenAPI_KMA_ASOS_Result.IsAlive)
                {
                    thOpenAPI_KMA_ASOS_Result.Abort();
                }

                thOpenAPI_KMA_ASOS = null;
                thOpenAPI_KMA_ASOS_Period = null;
                thOpenAPI_KMA_ASOS_Result = null;


                isServiceRunning = false; // 서비스 실행 상태 해제
                WriteStatus("모든 서비스가 중지되었습니다.");
                MessageBox.Show("ASOS 서비스가 성공적으로 중지되었습니다.", "서비스 상태", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"서비스 중지 중 오류 발생: {ex.Message}");
                MessageBox.Show($"서비스 중지 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenAPI_KMA_ASOS_ResultCaller()
        {
            WriteStatus(string.Format("ASOS Result Caller 실행"));

            while (true)
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
                    //결과 저장 로직
                    OpenAPIKMAASOSResultInsertProcess(resultPath);
                }
                else
                {
                    //파일명 없을때 Wait Signal
                    eventWaitHandle.WaitOne();
                }
            }
        }
        private void OpenAPIKMAASOSResultInsertProcess(string resultPath)
        {
            try
            {
                _logger.Debug($"파일 처리 시작: {resultPath}", "ResultProcess");
                DateTime processStart = DateTime.Now;

                // 파일명에서 관측소 ID와 날짜 추출
                string filePathWithoutExt = Path.GetFileNameWithoutExtension(resultPath);
                string[] splitedstring = filePathWithoutExt.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                int stdID = int.Parse(splitedstring[1].Trim());
                string fileDate = splitedstring[2].Trim(); // 파일명의 날짜

                // 파일 데이터 읽기
                List<rcvKMAASOSData> listKMAASOS = KMA_Controller.FiletoList_KMAASOS(resultPath);
                if (listKMAASOS == null || listKMAASOS.Count == 0)
                {
                    _logger.Warning($"파일에서 데이터를 찾을 수 없습니다: {resultPath}", "ResultProcess");
                    return;
                }

                _logger.Info($"파일에서 {listKMAASOS.Count}개 데이터 로드 완료", "ResultProcess");

                // 파일명의 날짜를 기준으로 처리
                DateTime sDate = BizCommon.StringtoDateTimeStart(fileDate);
                DateTime eDate = BizCommon.StringtoDateTimeEnd(fileDate);
                _logger.Debug($"파일 정보: 관측소ID={stdID}, 날짜={fileDate}, 처리일자={sDate:yyyy-MM-dd}", "ResultProcess");

                // 해당 날짜의 기존 데이터 조회
                DateTime dbQueryStart = DateTime.Now;
                List<rcvKMAASOSData> listKMAASOS_Database = NpgSQLService.GetDailyDatas_FromOpenAPI_KMAASOS(stdID, sDate, eDate);
                TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;
                _logger.LogPerformance("기존 데이터 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");
                _logger.Debug($"기존 데이터 조회 결과: {listKMAASOS_Database.Count}건", "ResultProcess");

                // 새로운 데이터만 필터링
                List<rcvKMAASOSData> addDatas = listKMAASOS.Where(current =>
                    !listKMAASOS_Database.Any(db => db.TM == current.TM && db.STN == current.STN)).ToList();
                _logger.Info($"날짜 {fileDate}, 새로 추가할 데이터: {addDatas.Count}건", "ResultProcess");

                if (NpgSQLService.BulkInsert_KMAASOSDatas(addDatas) == true)
                {
                    // Bulk Insert 실행
                    _logger.Info($"데이터베이스에 {addDatas.Count}건 저장 중...", "Database");
                    DateTime dbInsertStart = DateTime.Now;
                    bool result = NpgSQLService.BulkInsert_KMAASOSDatas(addDatas);
                    TimeSpan dbInsertDuration = DateTime.Now - dbInsertStart;
                    _logger.LogPerformance("데이터베이스 삽입", (long)dbInsertDuration.TotalMilliseconds, "Database");

                    if (result)
                    {
                        _logger.Info($"데이터베이스 저장 성공: 날짜={fileDate}, {addDatas.Count}건", "Database");
                    }
                    else
                    {
                        _logger.Error($"데이터베이스 저장 실패: 날짜={fileDate}, {addDatas.Count}건", "Database");
                    }
                }
                else
                {
                    _logger.Info($"날짜 {fileDate}, 저장할 새로운 데이터가 없습니다.", "ResultProcess");
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"OpenAPIKMAASOSResultInsertProcess 예외 발생: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
            }
        }
        //private void OpenAPIKMAASOSResultInsertProcess(string resultPath)
        //{
        //    try
        //    {
        //        _logger.Debug($"파일 처리 시작: {resultPath}", "ResultProcess");
        //        DateTime processStart = DateTime.Now;

        //        List<rcvKMAASOSData> listKMAASOS = new List<rcvKMAASOSData>();
        //        listKMAASOS = KMA_Controller.FiletoList_KMAASOS(resultPath);

        //        if (listKMAASOS == null || listKMAASOS.Count == 0)
        //        {
        //            _logger.Warning($"파일에서 데이터를 찾을 수 없습니다: {resultPath}", "ResultProcess");
        //            return;
        //        }

        //        _logger.Info($"파일에서 {listKMAASOS.Count}개 데이터 로드 완료", "ResultProcess");

        //        string filePathWithoutExt = Path.GetFileNameWithoutExtension(resultPath);
        //        //파일명에서 정보추출
        //        string[] splitedstring = filePathWithoutExt.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        //        int stdID = int.Parse(splitedstring[1].Trim());
        //        DateTime sDate = BizCommon.StringtoDateTimeStart(splitedstring[2].Trim());
        //        DateTime eDate = BizCommon.StringtoDateTimeEnd(splitedstring[2].Trim());

        //        _logger.Debug($"파일 정보: 관측소ID={stdID}, 시작일={sDate:yyyy-MM-dd}, 종료일={eDate:yyyy-MM-dd}", "ResultProcess");

        //        //입력 동일기간의 데이터를 Database에서 조회
        //        DateTime dbQueryStart = DateTime.Now;
        //        List<rcvKMAASOSData> listKMAASOS_Database = new List<rcvKMAASOSData>();
        //        listKMAASOS_Database = NpgSQLService.GetDailyDatas_FromOpenAPI_KMAASOS(stdID, sDate, eDate);
        //        TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;

        //        _logger.LogPerformance("기존 데이터 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");
        //        _logger.Debug($"기존 데이터 조회 결과: {listKMAASOS_Database.Count}건", "ResultProcess");

        //        //Database와 비교하여 Database에 없는것 입력
        //        List<rcvKMAASOSData> addDatas = listKMAASOS.Where(current =>
        //            !listKMAASOS_Database.Any(db => db.TM == current.TM && db.STN == current.STN)).ToList();

        //        _logger.Info($"새로 추가할 데이터: {addDatas.Count}건", "ResultProcess");

        //        if (addDatas.Count > 0)
        //        {
        //            //Bulk Insert 실행
        //            _logger.Info($"데이터베이스에 {addDatas.Count}건 저장 중...", "Database");

        //            DateTime dbInsertStart = DateTime.Now;
        //            bool result = NpgSQLService.BulkInsert_KMAASOSDatas(addDatas);
        //            TimeSpan dbInsertDuration = DateTime.Now - dbInsertStart;

        //            _logger.LogPerformance("데이터베이스 삽입", (long)dbInsertDuration.TotalMilliseconds, "Database");

        //            if (result)
        //            {
        //                _logger.Info($"데이터베이스 저장 성공: 관측소ID={stdID}, {addDatas.Count}건", "Database");
        //            }
        //            else
        //            {
        //                _logger.Error($"데이터베이스 저장 실패: 관측소ID={stdID}, {addDatas.Count}건", "Database");
        //            }
        //        }
        //        else
        //        {
        //            _logger.Info("저장할 새로운 데이터가 없습니다.", "ResultProcess");
        //        }

        //        TimeSpan totalDuration = DateTime.Now - processStart;
        //        _logger.LogPerformance("전체 처리 시간", (long)totalDuration.TotalMilliseconds, "ResultProcess");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex, "결과 처리 중 오류 발생", LogLevel.Error, "ResultProcess");
        //    }
        //}

        private void OpenAPI_KMA_ASOS_PeriodCaller()
        {
            WriteStatus("기간별 ASOS 데이터 수집 시작");
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
                    WriteStatus("종료일이 오늘 이후로 설정되어 있어 오늘까지만 처리합니다.");
                }

                int totalDays = (reqEndDate - reqStartDate).Days + 1;
                int processedDays = 0;
                int dataFoundDays = 0;

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

                    WriteStatus(string.Format("요청 중... 날짜: {0}", tm));

                    Uri uri = new Uri(string.Format("{0}tm={1}&stn={2}&disp={3}&help={4}&authKey={5}",
                        serviceURL, tm, stn, disp, help, authKey));

                    string filePath = KMA_Controller.ExecuteDownloadResponse(uri, tm, stn);

                    if (filePath != string.Empty)
                    {
                        WriteStatus(string.Format("파일 생성 완료: {0}", filePath));
                        EnqueueOpenAPIKMAASOSResult(filePath);
                        dataFoundDays++;
                    }
                    else
                    {
                        WriteStatus(string.Format("{0} 날짜의 데이터가 없습니다", tm));
                    }

                    processedDays++;
                }

                string completionMessage = string.Format(
                    "기간별 데이터 수집 완료: 총 {0}일 중 {1}일의 데이터를 찾았습니다.",
                    totalDays, dataFoundDays);
                WriteStatus(completionMessage);

                // 작업 완료 메시지 표시
                this.Invoke(new Action(() =>
                    MessageBox.Show(completionMessage, "기간별 수집 완료", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ));
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"OpenAPI_KMA_ASOS_PeriodCaller 예외 발생: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                WriteStatus($"기간별 ASOS 데이터 수집 중 오류 발생: {ex.Message}");

                // 오류 발생 시 메시지 박스로 알림
                this.Invoke(new Action(() =>
                    MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ));
            }
        }
        private void OpenAPI_KMA_ASOS_AutoCaller()
        {
            //ASOS일데이터에 맞게 호출시간 변경
            int nTimeGap = 1000 * Config.KMA_ASOS_Auto_Caller_Second;

            deleKMAASOS_AutoCaller deleKMAASOSMethod = new deleKMAASOS_AutoCaller(this.OpenAPI_KMA_ASOS);

            while (true)
            {
                IAsyncResult ar = deleKMAASOSMethod.BeginInvoke(null, null);
                Thread.Sleep(nTimeGap);
            }
        }

        private void OpenAPI_KMA_ASOS()
        {
            WriteStatus("OpenAPI KMA ASOS Service 시작됨");
            try
            {
                #region [비동기 설정]
                #endregion
                #region [데이터 수집 Layer]
                // 관측소 조회
                List<KMASiteInformation> listAWS = new List<KMASiteInformation>();
                listAWS = NpgSQLService.GetSites_FromOpenAPI_KMAASOS();

                //관측소별 데이터요청 (파일로 다운)
                if (listAWS != null)
                {
                    // 데이터베이스에서 tm의 최종 일자를 조회
                    DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_KMAASOS();
                    DateTime today = DateTime.Today;

                    // 최종 데이터가 오늘 날짜보다 이후인 경우 작업 취소
                    if (lastDate.Date >= today.Date)
                    {
                        string message = string.Format("최종 데이터({0})가 오늘자입니다. 데이터 수집을 건너뜁니다.",
                            lastDate.ToString("yyyy-MM-dd"));
                        WriteStatus(message);

                        // 메시지 박스로 알림 표시
                        this.Invoke(new Action(() =>
                            MessageBox.Show(message, "데이터 수집 알림", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ));

                        return; // 메서드 종료
                    }

                    // 최종 일자부터 오늘까지의 데이터를 요청
                    for (DateTime date = lastDate.AddDays(1); date <= today; date = date.AddDays(1))
                    {
                        string tm = date.ToString("yyyyMMdd");
                     //   string tm2 = today.ToString("yyyyMMdd");

                        WriteStatus(string.Format("{0} 날짜의 데이터 수집 시작", tm));

                        string serviceURL = "https://apihub-pub.kma.go.kr/api/typ01/url/kma_sfcdd.php?";
                        string stn = "0";
                        string disp = "1";
                        string help = "0";
                        string authKey = "40cfe353913cd680317889498823f9214c0d7a09e09583a5b09291467c37af3414233051807b848caa2d0162742948c28969c222eb3dcdfc061abea91ac9d60a";

                        // API 호출 시작 시간 기록
                        DateTime apiCallStart = DateTime.Now;

                        //Uri uri = new Uri(string.Format("{0}tm={1}&tm2={2}&stn={3}&disp={4}&help={5}&authKey={6}",
                        //    serviceURL, tm, tm2, stn, disp, help, authKey));
                        Uri uri = new Uri(string.Format("{0}tm={1}&stn={2}&disp={3}&help={4}&authKey={5}",
                    serviceURL, tm, stn, disp, help, authKey));

                        string filePath = KMA_Controller.ExecuteDownloadResponse(uri, tm, stn);

                        #region [데이터 입력 Layer]
                        //데이터 들어온거 확인
                        if (filePath != string.Empty)
                        {
                            WriteStatus(string.Format("{0} 날짜의 데이터 수집 완료", tm));
                            EnqueueOpenAPIKMAASOSResult(filePath);
                        }
                        else
                        {
                            string noDataMessage = string.Format("{0} 날짜의 데이터가 없습니다", tm);
                            WriteStatus(noDataMessage);

                            // 데이터가 없을 경우 메시지 박스로 알림 (선택적)
                            // this.Invoke(new Action(() => 
                            //     MessageBox.Show(noDataMessage, "데이터 없음 알림", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            // ));
                        }
                        #endregion
                    }
                }
                #endregion

                WriteStatus("OpenAPI KMA ASOS Service 완료됨");

                // 작업 완료 메시지 표시
                this.Invoke(new Action(() =>
                    MessageBox.Show("ASOS 데이터 수집이 완료되었습니다.", "작업 완료", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ));
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"OpenAPI_KMA_ASOS 예외 발생: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                WriteStatus($"OpenAPI KMA ASOS Service 오류 발생: {ex.Message}");

                // 오류 발생 시 메시지 박스로 알림
                this.Invoke(new Action(() =>
                    MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ));
            }
        }
        public static void CollectRealTimeData(Uri baseUrl, string stn)
        {
            try
            {
                // 마지막 DB 날짜 가져오기
                DateTime lastDbDate = NpgSQLService.GetLastDateFromOpenAPI_KMAASOS();
                GMLogHelper.WriteLog($"마지막 DB 날짜: {lastDbDate.ToString("yyyyMMdd")}");

                // 현재 날짜 가져오기
                DateTime currentDate = DateTime.Today;
                GMLogHelper.WriteLog($"현재 날짜: {currentDate.ToString("yyyyMMdd")}");

                // 처리할 날짜 범위 계산
                int totalDays = (int)(currentDate - lastDbDate).TotalDays;
                if (totalDays <= 0)
                {
                    GMLogHelper.WriteLog("이미 최신 데이터가 있습니다.");
                    return;
                }

                GMLogHelper.WriteLog($"처리할 날짜 범위: {lastDbDate.AddDays(1).ToString("yyyyMMdd")} ~ {currentDate.ToString("yyyyMMdd")} ({totalDays}일)");

                // 날짜 범위 처리
                int processedDays = 0;
                int successDays = 0;

                // 마지막 DB 날짜의 다음 날부터 현재 날짜까지 반복
                for (DateTime date = lastDbDate.AddDays(1); date <= currentDate; date = date.AddDays(1))
                {
                    string dateStr = date.ToString("yyyyMMdd");
                    GMLogHelper.WriteLog($"{dateStr} 날짜의 데이터 수집 시작");

                    // 해당 날짜에 대한 데이터 수집
                    string filePath = KMA_Controller.ExecuteDownloadResponse(baseUrl, dateStr, stn);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        // CSV 파일 처리
                        List<rcvKMAASOSData> dataList = KMA_Controller.FiletoList_KMAASOS(filePath);

                        // DB에 데이터 삽입
                        if (dataList.Count > 0)
                        {
                            if (NpgSQLService.BulkInsert_KMAASOSDatas(dataList))
                            {
                                successDays++;
                                GMLogHelper.WriteLog($"{dateStr} 날짜의 데이터 수집 및 DB 입력 완료 (데이터 수: {dataList.Count})");
                            }
                            else
                            {
                                GMLogHelper.WriteLog($"{dateStr} 날짜의 데이터 DB 입력 실패");
                            }
                        }
                        else
                        {
                            GMLogHelper.WriteLog($"{dateStr} 날짜의 데이터가 없습니다.");
                        }
                    }
                    else
                    {
                        GMLogHelper.WriteLog($"{dateStr} 날짜의 데이터 다운로드 실패");
                    }

                    processedDays++;

                    // 진행 상황 업데이트
                    int progressPercentage = (int)((double)processedDays / totalDays * 100);
                    GMLogHelper.WriteLog($"진행 상황: {processedDays}/{totalDays}일 처리 완료 ({progressPercentage}%)");
                }

                GMLogHelper.WriteLog($"실시간 데이터 수집 완료: 총 {totalDays}일 중 {successDays}일의 데이터를 DB에 입력했습니다.");
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"데이터 수집 중 오류 발생: {ex.Message}");
                GMLogHelper.WriteLog($"스택 트레이스: {ex.StackTrace}");
            }
        }
        private void EnqueueOpenAPIKMAASOSResult(string filePath)
        {
            //결과 파일명 Queue에 넣기
            lock (locker) OpenAPI_KMA_ASOS_ResultQueue.Enqueue(filePath);
            //결과 처리하도록 이벤트 발생
            eventWaitHandle.Set();
        }
        #endregion

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
                // 리스트박스가 비어있고 "표시할 데이터가 없습니다" 메시지가 있으면 제거
                if (listStatus.Items.Count == 1 && listStatus.Items[0].ToString().Contains("표시할 데이터가 없습니다"))
                {
                    listStatus.Items.Clear();
                }

                // 새 메시지 추가
                string formattedMessage = string.Format("{0} - {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
                listStatus.Items.Add(formattedMessage);

                // 자동 스크롤을 위해 마지막 항목 선택
                listStatus.SelectedIndex = listStatus.Items.Count - 1;
                listStatus.ClearSelected();

                // 로그에도 기록
                GMLogHelper.WriteLog(message);
            }
        }

        // 리스트박스가 비어있는지 확인하는 메서드
        private void CheckListBoxEmpty()
        {
            if (listStatus.Items.Count == 0)
            {
                listStatus.Items.Add("표시할 데이터가 없습니다.");
            }
        }
        #endregion HEAD

        #region [메뉴]
        //private void periodSettingToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    ShowForm(typeof(frmConfig), true, true);
        //}

        #endregion

        #region [메뉴]
        private void periodSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        private void AllCloseTabs()
        {
            foreach (var frm in this.MdiChildren)
            {
                frm.Close();
            }
        }



        #endregion

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
        
}
