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
        
void OpenAPIKMAASOSResultInsertProcess(string resultPath)
        {
            try
            {
                List<rcvKMAASOSData> listKMAASOS = new List<rcvKMAASOSData>();
                listKMAASOS = KMA_Controller.FiletoList_KMAASOS(resultPath);

                string filePathWithoutExt = Path.GetFileNameWithoutExtension(resultPath);

                //파일명에서 정보추출
                string[] splitedstring = filePathWithoutExt.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                int stdID = int.Parse(splitedstring[1].Trim());
                DateTime sDate = BizCommon.StringtoDateTimeStart(splitedstring[2].Trim());
                DateTime eDate = BizCommon.StringtoDateTimeEnd(splitedstring[2].Trim());

                //입력 동일기간의 데이터를 Database에서 조회
                List<rcvKMAASOSData> listKMAASOS_Database = new List<rcvKMAASOSData>();
                listKMAASOS_Database = NpgSQLService.GetDailyDatas_FromOpenAPI_KMAASOS(stdID, sDate, eDate);

                //Database와 비교하여 Database에 없는것 입력
                List<rcvKMAASOSData> addDatas = listKMAASOS.Where(current => !listKMAASOS_Database.Any(db => db.TM == current.TM && db.STN == current.STN)).ToList();

                //Bulk Insert 실행
                WriteStatus(string.Format("Insert process...{0} ea", addDatas.Count));

                if (NpgSQLService.BulkInsert_KMAASOSDatas(addDatas) == true)
                {
                    WriteStatus(string.Format("Database Insert... {0}:site {1}:Datas Success", stdID, addDatas.Count));
                }
                else
                {
                    WriteStatus(string.Format("Database Insert... {0}:site {1}:Datas fail", stdID, addDatas.Count));
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"OpenAPIKMAASOSResultInsertProcess 예외 발생: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
            }
        }

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
                        string tm2 = today.ToString("yyyyMMdd");

                        WriteStatus(string.Format("{0} 날짜의 데이터 수집 시작", tm));

                        string serviceURL = "https://apihub-pub.kma.go.kr/api/typ01/url/kma_sfcdd3.php?";
                        string stn = "0";
                        string disp = "1";
                        string help = "0";
                        string authKey = "40cfe353913cd680317889498823f9214c0d7a09e09583a5b09291467c37af3414233051807b848caa2d0162742948c28969c222eb3dcdfc061abea91ac9d60a";

                        Uri uri = new Uri(string.Format("{0}tm={1}&tm2={2}&stn={3}&disp={4}&help={5}&authKey={6}",
                            serviceURL, tm, tm2, stn, disp, help, authKey));

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
        #endregion
        }

            }
        }



        #endregion
}
