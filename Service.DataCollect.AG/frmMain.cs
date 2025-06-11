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
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UFRI.FrameWork;
using UFRI.FramWork;

namespace Service.DataCollect.AG
{
    public partial class frmMain : Form
    {
        #region [Delegate]
        delegate void deleOpenAPI_AG_tb_reserviorlevel_Caller();
        #endregion

        #region [WorkerThread]
        private EventWaitHandle eventWaitHandle = new AutoResetEvent(false);
        private readonly object locker = new object();
        private Queue<List<ReservoirLevelData>> OpenAPI_AG_tb_reserviorlevel_ResultQueue = new Queue<List<ReservoirLevelData>>();
        #endregion

        #region [Variables]
        public Global _global { get; set; }
        private bool isServiceRunning { get; set; }
        private volatile bool _shouldStop = false;
        private LogManager _logger; // 로그 관리자 인스턴스
        #endregion

        #region [Threads]
        private Thread thOpenAPI_AG_tb_reserviorlevel { get; set; }
        private Thread thOpenAPI_AG_tb_reserviorlevel_Period { get; set; }
        private Thread thOpenAPI_AG_tb_reserviorlevel_Result { get; set; }
        #endregion

        public frmMain()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // 로그 관리자 초기화
            _logger = LogManager.GetInstance();
            _logger.Initialize(listStatus, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"), "AG");
            _logger.Info("애플리케이션 시작", "System");
;
            InitializeVariables();

            if (InitializeDatabase())
            {
                _logger.Info("데이터베이스 초기화 성공", "Database");
            }
            else
            {
                _logger.Error("데이터베이스 초기화 실패", "Database");
            }
        }



        private void InitializeVariables()
        {
            this.isServiceRunning = false;
            _global.RealTimeUse = BizCommon.BoolConvert(Config.RealTimeUse);
            _global.PeriodUse = BizCommon.BoolConvert(Config.PeriodUse);
            _global.startDate = new DateTime(Config.StartDate, 1, 1);
            _global.endDate = DateTime.Today;

            _logger.Debug($"기간 사용: {_global.PeriodUse}, 시작일: {_global.startDate:yyyy-MM-dd}, 종료일: {_global.endDate:yyyy-MM-dd}", "Config");
        }

        /// <summary>
        /// 데이터베이스 연결을 초기화하고 테스트합니다.
        /// </summary>
        /// <returns></returns>
        private bool InitializeDatabase()
        {
            _logger.Info("데이터베이스 연결 테스트 중...", "Database");

            if (NpgSQLService.TestConnection())
            {
                _logger.Info("데이터베이스 연결 성공.", "Database");
                return true;
            }
            else
            {
                _logger.Error("데이터베이스 연결 실패.", "Database");
                return false;
            }
        }
        //private bool InitializeDatabase()
        //{
        //    // 데이터베이스 연결 초기화 로직
        //    _logger.Info("데이터베이스 연결 시도 중...", "Database");

        //    string dbIP = Config.dbIP;
        //    string dbName = Config.dbName;
        //    string dbPort = Config.dbPort;
        //    string dbId = Config.dbId;

        //    _logger.Debug($"DB 연결 정보: IP={dbIP}, DB={dbName}, Port={dbPort}, ID={dbId}", "Database");

        //    try
        //    {
        //        string strConn = $"Server={dbIP};Port={dbPort};User Id={dbId};Password={Config.dbPassword};Database={dbName};";

        //        using (NpgsqlConnection conn = new NpgsqlConnection(strConn))
        //        {
        //            conn.Open();
        //            _logger.Info("데이터베이스 연결 성공", "Database");
        //            conn.Close();
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error($"데이터베이스 연결 실패: {ex.Message}", "Database");
        //        _logger.Debug($"연결 오류 상세: {ex.StackTrace}", "Database");
        //        return false;
        //    }
        //}

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isServiceRunning)
            {
                _logger.Info("서비스 시작 요청", "Service");
                bool success = ServiceStart();
                if (success)
                {
                    _logger.Info("서비스가 성공적으로 시작되었습니다.", "Service");
                    btnStart.Enabled = false;
                    btnStop.Enabled = true;
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
            if (isServiceRunning)
            {
                _logger.Info("서비스 중지 요청", "Service");
                ServiceStop();
                _logger.Info("서비스가 중지되었습니다.", "Service");
                btnStart.Enabled = true;
                btnStop.Enabled = false;
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
                if (_global.RealTimeUse)
                {
                    _logger.Info("실시간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_AG_tb_reserviorlevel = new Thread(OpenAPI_AG_tb_reserviorlevel_AutoCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_AG_tb_reserviorlevel.Start();
                }

                if (_global.PeriodUse)
                {
                    _logger.Info("기간 데이터 수집 스레드 시작", "Thread");
                    thOpenAPI_AG_tb_reserviorlevel_Period = new Thread(OpenAPI_AG_tb_reserviorlevel_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_AG_tb_reserviorlevel_Period.Start();
                }

                _logger.Info("결과 처리 스레드 시작", "Thread");
                thOpenAPI_AG_tb_reserviorlevel_Result = new Thread(OpenAPI_AG_tb_reserviorlevel_ResultCaller)
                {
                    IsBackground = true
                };
                thOpenAPI_AG_tb_reserviorlevel_Result.Start();

                isServiceRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 시작 중 오류 발생", LogLevel.Error, "Service");
                return false;
            }
        }

        private void OpenAPI_AG_tb_reserviorlevel_AutoCaller()
        {
            int nTimeGap = 1000 * Config.AG_tb_reserviorlevel_Auto_Caller_Second;
            _logger.Info($"실시간 데이터 수집 간격: {Config.AG_tb_reserviorlevel_Auto_Caller_Second}초", "AutoCaller");

            deleOpenAPI_AG_tb_reserviorlevel_Caller deleMethod = new deleOpenAPI_AG_tb_reserviorlevel_Caller(this.OpenAPI_AG_tb_reserviorlevel_Service);

            while (!_shouldStop)
            {
                _logger.Debug("실시간 데이터 수집 호출 시작", "AutoCaller");
                IAsyncResult ar = deleMethod.BeginInvoke(null, null);
                Thread.Sleep(nTimeGap);
            }

            _logger.Info("실시간 데이터 수집 스레드 종료", "AutoCaller");
        }

        private void OpenAPI_AG_tb_reserviorlevel_Service()
        {

            try
            {
                _logger.Info("농업용 저수지 실시간 데이터 수집 모듈 시작", "Service");

                // DB에서 최종 데이터 일자 조회
                DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_AG_tb_reserviorlevel();
                DateTime today = DateTime.Today;

                _logger.Info($"최종 데이터 일자: {lastDate:yyyy-MM-dd}, 오늘 날짜: {today:yyyy-MM-dd}", "Service");

                // 최종 데이터가 오늘 날짜보다 이후인 경우 작업 취소
                if (lastDate.Date >= today.Date)
                {
                    string message = $"최종 데이터({lastDate:yyyy-MM-dd})가 오늘자입니다. 데이터 수집을 건너뜁니다.";
                    _logger.Warning(message, "Service");
                    return; // 메서드 종료
                }

                // 농업용 저수지 정보 가져오기
                List<AgriDamSpec> listAgriDam = NpgSQLService.Get_AgriDamSpec();

                if (listAgriDam != null && listAgriDam.Count > 0)
                {
                    _logger.Info($"농업용 저수지 {listAgriDam.Count}개 정보 로드 완료", "Service");

                    // 최종일자 다음날부터 오늘까지 한 번에 요청
                    DateTime startDate = lastDate.AddDays(1);
                    _logger.Info($"데이터 수집 시작: {startDate:yyyy-MM-dd} ~ {today:yyyy-MM-dd}", "Service");

                    int totalProcessed = 0;
                    int totalSuccess = 0;

                    foreach (AgriDamSpec dam in listAgriDam)
                    {
                        if (_shouldStop) break;

                        try
                        {
                            _logger.Info($"[{dam.facName}] 저수지 데이터 조회 중... ({startDate:yyyy-MM-dd} ~ {today:yyyy-MM-dd})", "API");

                            DateTime apiCallStart = DateTime.Now;

                            // API 호출하여 데이터 가져오기 - 기간 전체를 한 번에 요청
                            List<ReservoirLevelData> data = GetReservoirDataAsync(dam.facCode, startDate, today).Result;

                            TimeSpan apiCallDuration = DateTime.Now - apiCallStart; //호출 소요시간 축정
                            _logger.LogPerformance($"[{dam.facName}] API 호출", (long)apiCallDuration.TotalMilliseconds);

                            if (data != null && data.Count > 0)
                            {
                                // 결과 큐에 추가
                                lock (locker)
                                {
                                    OpenAPI_AG_tb_reserviorlevel_ResultQueue.Enqueue(data);
                                }

                                // 결과 처리 이벤트 발생
                                eventWaitHandle.Set();

                                _logger.Info($"[{dam.facName}] 저수지 데이터 {data.Count}건 처리 완료", "API");
                                totalSuccess++;
                                totalProcessed += data.Count;
                            }
                            else
                            {
                                _logger.Warning($"[{dam.facName}] 저수지 데이터가 없습니다.", "API");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex, $"[{dam.facName}] 저수지 데이터 처리 중 오류 발생", LogLevel.Error, "API");
                        }

                        // API 호출 간 딜레이 추가 (과도한 요청 방지)
                        Thread.Sleep(100);
                    }

                    _logger.Info($"실시간 데이터 수집 완료: 총 {listAgriDam.Count}개 저수지 중 {totalSuccess}개 성공, {totalProcessed}건 데이터 처리", "Service");
                }
                else
                {
                    _logger.Error("저수지 정보를 가져오는데 실패했습니다.", "Service");
                }

                _logger.Info("농업용 저수지 실시간 데이터 수집 모듈 종료", "Service");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "농업용 저수지 실시간 데이터 수집 모듈 오류", LogLevel.Error, "Service");
            }
        }

        private async void OpenAPI_AG_tb_reserviorlevel_PeriodCaller()
        {
            try
            {
                _logger.Info("농업용 저수지 기간 조회 모듈 시작", "PeriodCaller");

                // 선택된 기간 가져오기
                DateTime startDate = _global.startDate;
                DateTime endDate = _global.endDate;
                DateTime today = DateTime.Today;

                _logger.Info($"설정된 기간: {startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}", "PeriodCaller");

                // 종료일이 오늘 날짜를 초과하는지 확인
                if (endDate > today)
                {
                    _logger.Warning("종료일은 오늘 날짜를 초과할 수 없습니다.", "PeriodCaller");
                    endDate = today;
                    _logger.Info($"종료일을 오늘({today:yyyy-MM-dd})로 조정합니다.", "PeriodCaller");
                }

                // 기간이 365일을 초과하는지 확인
                TimeSpan dateDiff = endDate - startDate;
                if (dateDiff.TotalDays > 365)
                {
                    _logger.Warning("조회 기간은 최대 365일까지 가능합니다. 1년 단위로 나누어 조회합니다.", "PeriodCaller");

                    // 시작일부터 1년씩 증가하면서 조회
                    DateTime currentStartDate = startDate;
                    while (currentStartDate < endDate)
                    {
                        // 현재 시작일로부터 정확히 1년 후 날짜 계산 (같은 월/일로 설정)
                        DateTime currentEndDate = new DateTime(currentStartDate.Year + 1, currentStartDate.Month, currentStartDate.Day).AddDays(-1);

                        // 계산된 종료일이 실제 종료일을 초과하면 실제 종료일로 조정
                        if (currentEndDate > endDate)
                        {
                            currentEndDate = endDate;
                        }

                        // 계산된 종료일이 오늘 날짜를 초과하면 오늘 날짜로 조정
                        if (currentEndDate > today)
                        {
                            currentEndDate = today;
                        }

                        _logger.Info($"기간 조회: {currentStartDate:yyyy-MM-dd} ~ {currentEndDate:yyyy-MM-dd}", "PeriodCaller");

                        // 여기서 현재 기간에 대한 데이터 조회 로직 실행
                        await ProcessPeriodData(currentStartDate, currentEndDate);

                        // 다음 시작일 설정 (현재 종료일 다음날)
                        currentStartDate = currentEndDate.AddDays(1);
                    }
                }
                else
                {
                    // 365일 이내인 경우 한 번에 조회
                    _logger.Info($"기간 조회: {startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}", "PeriodCaller");

                    // 여기서 전체 기간에 대한 데이터 조회 로직 실행
                    await ProcessPeriodData(startDate, endDate);
                }

                _logger.Info("농업용 저수지 기간 조회 모듈 종료", "PeriodCaller");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "농업용 저수지 기간 조회 모듈 오류", LogLevel.Error, "PeriodCaller");
            }
        }

        private async Task ProcessPeriodData(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 농업용 저수지 정보 가져오기
                List<AgriDamSpec> listAgriDam = NpgSQLService.Get_AgriDamSpec();

                if (listAgriDam != null && listAgriDam.Count > 0)
                {
                    _logger.Info($"농업용 저수지 {listAgriDam.Count}개 정보 로드 완료", "PeriodData");

                    int totalProcessed = 0;
                    int totalSuccess = 0;

                    foreach (AgriDamSpec dam in listAgriDam)
                    {
                        if (_shouldStop) break;

                        try
                        {
                            _logger.Info($"[{dam.facName}] 저수지 데이터 조회 중... ({startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd})", "PeriodData");

                            // API 호출 시작 시간 기록
                            DateTime apiCallStart = DateTime.Now;

                            // API 호출하여 데이터 가져오기
                            List<ReservoirLevelData> data = await GetReservoirDataAsync(dam.facCode, startDate, endDate);

                            // API 호출 소요 시간 계산
                            TimeSpan apiCallDuration = DateTime.Now - apiCallStart;
                            _logger.LogPerformance($"[{dam.facName}] API 호출", (long)apiCallDuration.TotalMilliseconds);

                            if (data != null && data.Count > 0)
                            {
                                // 결과 큐에 추가
                                lock (locker)
                                {
                                    OpenAPI_AG_tb_reserviorlevel_ResultQueue.Enqueue(data);
                                }

                                // 결과 처리 이벤트 발생
                                eventWaitHandle.Set();

                                _logger.Info($"[{dam.facName}] 저수지 데이터 {data.Count}건 처리 완료", "PeriodData");
                                totalSuccess++;
                                totalProcessed += data.Count;
                            }
                            else
                            {
                                _logger.Warning($"[{dam.facName}] 저수지 데이터가 없습니다.", "PeriodData");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex, $"[{dam.facName}] 저수지 데이터 처리 중 오류 발생", LogLevel.Error, "PeriodData");
                        }

                        // API 호출 간 딜레이 추가 (과도한 요청 방지)
                        await Task.Delay(100);
                    }

                    _logger.Info($"기간 데이터 수집 완료: 총 {listAgriDam.Count}개 저수지 중 {totalSuccess}개 성공, {totalProcessed}건 데이터 처리", "PeriodData");
                }
                else
                {
                    _logger.Error("저수지 정보를 가져오는데 실패했습니다.", "PeriodData");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "기간 데이터 처리 중 오류 발생", LogLevel.Error, "PeriodData");
            }
        }

        //private async Task<List<ReservoirLevelData>> GetReservoirDataAsync(string damCode, DateTime startDate, DateTime endDate)
        //{
        //    List<ReservoirLevelData> result = new List<ReservoirLevelData>();
        //    try
        //    {
        //        string formattedStartDate = startDate.AddDays(-1).ToString("yyyyMMdd"); //최소기간조회를 위한 -1일  
        //        string formattedEndDate = endDate.ToString("yyyyMMdd");

        //        // API URL 및 키 설정
        //        string apiUrl = "http://apis.data.go.kr/B552149/reserviorWaterLevel/reservoirlevel/"; 
        //        //string ApiKey = Config.AG_ApiKey2; // DATA_ApiKey1~3 중 한개 사용
        //        //string apiKey = "FpAShNYZ~~
        //        //string apiKey2 = "wN6RP55~~//보조용(목)
        //        //string apiKey3 = "TeSSIf1TY~~//보조용(주)


        //        string requestUrl = $"{apiUrl}?serviceKey={Config.DATA_ApiKey2}&pageNo=1&numOfRows=1000&fac_code={damCode}&date_s={formattedStartDate}&date_e={formattedEndDate}";

        //        _logger.Debug($"API 요청 URL: {requestUrl}", "API");

        //        // HttpClient 생성
        //        using (HttpClient httpClient = new HttpClient())
        //        {
        //            // 응답을 바이트 배열로 받아서 처리
        //            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

        //            _logger.Debug($"API 응답 상태 코드: {(int)response.StatusCode} ({response.StatusCode})", "API");

        //            response.EnsureSuccessStatusCode();
        //            byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
        //            string xmlResponse = Encoding.UTF8.GetString(byteArray);

        //            XmlDocument xmlDoc = new XmlDocument();
        //            xmlDoc.LoadXml(xmlResponse);
        //            XmlNode authMsgNode = xmlDoc.SelectSingleNode("/response/header/returnAuthMsg");
        //            XmlNode reasonCodeNode = xmlDoc.SelectSingleNode("/response/header/returnReasonCode");
        //            if (reasonCodeNode != null && reasonCodeNode.InnerText != "00")
        //            {
        //                string msg = $"API 응답 확인 필요: returnAuthMsg={authMsgNode?.InnerText}, returnReasonCode={reasonCodeNode.InnerText}, damCode={damCode}, 기간={formattedStartDate}~{formattedEndDate}";
        //                _logger.Warning(msg, "API");
        //            }

        //            XmlNodeList itemNodes = xmlDoc.SelectNodes("//item");

        //            if (itemNodes != null && itemNodes.Count > 0)
        //            {
        //                _logger.Debug($"API 응답에서 {itemNodes.Count}개 항목 발견", "API");

        //                foreach (XmlNode item in itemNodes)
        //                {
        //                    var data = new ReservoirLevelData
        //                    {
        //                        check_date = GetNodeValue(item, "check_date"),
        //                        county = GetNodeValue(item, "county"),
        //                        fac_code = GetNodeValue(item, "fac_code"),
        //                        fac_name = GetNodeValue(item, "fac_name"),
        //                        rate = GetNodeValue(item, "rate"),
        //                    };
        //                    result.Add(data);
        //                }
        //            }
        //            else
        //            {
        //                _logger.Warning($"API 응답에 데이터 항목이 없습니다. 저수지 코드: {damCode}", "API");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex, $"API 호출 오류 (저수지 코드: {damCode})", LogLevel.Error, "API");
        //    }

        //    return result;
        //}

        //private string GetNodeValue(XmlNode parentNode, string nodeName)
        //{
        //    XmlNode node = parentNode.SelectSingleNode(nodeName);
        //    return node != null ? node.InnerText : string.Empty;
        //}

        private void OpenAPI_AG_tb_reserviorlevel_ResultCaller()
        {
            _logger.Info("농업용 저수지 결과 처리 모듈 시작", "ResultCaller");

            while (!_shouldStop)
            {
                List<ReservoirLevelData> resultList = new List<ReservoirLevelData>();
                bool hasData = false;

                lock (locker)
                {
                    if (OpenAPI_AG_tb_reserviorlevel_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_AG_tb_reserviorlevel_ResultQueue.Dequeue();
                        hasData = true;
                    }
                }

                if (hasData && resultList.Count > 0)
                {
                    // 결과 저장 로직 구현
                    SaveReservoirDataToDatabase(resultList);
                }
                else
                {
                    // 데이터가 없을 때 이벤트 대기 (최대 100ms)
                    eventWaitHandle.WaitOne(100);
                }
            }

            _logger.Info("농업용 저수지 결과 처리 모듈 종료", "ResultCaller");
        }
        private bool SaveReservoirDataToDatabase(List<ReservoirLevelData> dataList)
        {
            if (dataList == null || !dataList.Any())
                return false;

            string facName = dataList.First().fac_name;
            _logger.Info($"[{facName}] 저수지 데이터 저장 시도: {dataList.Count}건", "Database");

            // NpgSQLService의 통합된 메서드를 호출하여 데이터 저장
            int insertedCount = NpgSQLService.UpsertReservoirLevelData(dataList);

            if (insertedCount > 0)
            {
                _logger.Info($"데이터베이스에 새로운 데이터 {insertedCount}건 저장 완료.", "Database");
                return true;
            }
            else if (insertedCount == 0)
            {
                _logger.Info("저장할 새로운 데이터가 없습니다.", "Database");
                return true;
            }
            else
            {
                _logger.Error("데이터베이스 저장 실패.", "Database");
                return false;
            }
        }
        //private bool SaveReservoirDataToDatabase(List<ReservoirLevelData> dataList)
        //{
        //    if (dataList == null || dataList.Count == 0)
        //        return false;

        //    try
        //    {
        //        // 기존 데이터 확인 (중복 방지)
        //        string facCode = dataList.First().fac_code;
        //        string startDate = dataList.Min(d => d.check_date);
        //        string endDate = dataList.Max(d => d.check_date);
        //        string facName = dataList.First().fac_name;

        //        _logger.Info($"[{facName}] 저수지 데이터 저장 준비: {dataList.Count}건 ({startDate} ~ {endDate})", "Database");

        //        // 기존 데이터 조회
        //        DateTime dbQueryStart = DateTime.Now;
        //        List<ReservoirLevelData> existingData = NpgSQLService.GetReservoirLevelData(facCode, startDate, endDate);
        //        TimeSpan dbQueryDuration = DateTime.Now - dbQueryStart;

        //        _logger.LogPerformance($"기존 데이터 조회", (long)dbQueryDuration.TotalMilliseconds, "Database");
        //        _logger.Debug($"기존 데이터 조회 결과: {existingData.Count}건", "Database");

        //        // 중복되지 않은 데이터만 필터링
        //        List<ReservoirLevelData> newData = dataList.Where(current =>
        //            !existingData.Any(db =>
        //                db.check_date == current.check_date &&
        //                db.fac_code == current.fac_code)
        //        ).ToList();

        //        if (newData.Count > 0)
        //        {
        //            // 벌크 삽입 실행
        //            _logger.Info($"데이터베이스에 {newData.Count}건 저장 중...", "Database");

        //            DateTime dbInsertStart = DateTime.Now;
        //            bool result = NpgSQLService.BulkInsert_ReservoirLevelData(newData);
        //            TimeSpan dbInsertDuration = DateTime.Now - dbInsertStart;

        //            _logger.LogPerformance($"데이터베이스 삽입", (long)dbInsertDuration.TotalMilliseconds, "Database");

        //            if (result)
        //            {
        //                _logger.Info($"데이터베이스에 {newData.Count}건 저장 완료", "Database");
        //                return true;
        //            }
        //            else
        //            {
        //                _logger.Error("데이터베이스 저장 실패", "Database");
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            _logger.Info("저장할 새로운 데이터가 없습니다.", "Database");
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex, "DB 저장 오류", LogLevel.Error, "Database");
        //        return false;
        //    }
        //}

        private void ServiceStop()
        {
            try
            {
                _logger.Info("서비스 중지 요청", "Service");

                _shouldStop = true;
                eventWaitHandle.Set();

                if (thOpenAPI_AG_tb_reserviorlevel != null && thOpenAPI_AG_tb_reserviorlevel.IsAlive)
                    thOpenAPI_AG_tb_reserviorlevel.Join(100);

                if (thOpenAPI_AG_tb_reserviorlevel_Period != null && thOpenAPI_AG_tb_reserviorlevel_Period.IsAlive)
                    thOpenAPI_AG_tb_reserviorlevel_Period.Join(100);

                if (thOpenAPI_AG_tb_reserviorlevel_Result != null && thOpenAPI_AG_tb_reserviorlevel_Result.IsAlive)
                    thOpenAPI_AG_tb_reserviorlevel_Result.Join(100);

                isServiceRunning = false;

                _logger.Info("서비스가 중지되었습니다.", "Service");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "서비스 중지 오류", LogLevel.Error, "Service");
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
                _logger.LogException(ex, "폼 표시 오류", LogLevel.Error, "UI");
                return DialogResult.None;
            }
        }
    }
}
#endregion
