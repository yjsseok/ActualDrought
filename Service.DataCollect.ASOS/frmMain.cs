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
                // SWMM 실행 스레드 종료
                if (thOpenAPI_KMA_ASOS != null && thOpenAPI_KMA_ASOS.IsAlive)
                {
                    thOpenAPI_KMA_ASOS.Abort();
                }

                isServiceRunning = false; // 서비스 실행 상태 해제
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"Error stopping service: {ex.Message}");
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

        private void OpenAPI_KMA_ASOS_PeriodCaller()
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

                WriteStatus(string.Format("Request... Date:{0}", tm));

                Uri uri = new Uri(string.Format("{0}tm={1}&stn={2}&disp={3}&help={4}&authKey={5}", serviceURL, tm, stn, disp, help, authKey));
                string filePath = KMA_Controller.ExecuteDownloadResponse(uri, tm, stn);

                WriteStatus(string.Format("makeFile... {0}", filePath));

                #region [데이터 입력 Layer]
                //데이터 들어온거 확인
                if (filePath != string.Empty)
                {
                    EnqueueOpenAPIKMAASOSResult(filePath);
                }
                #endregion
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
            WriteStatus("OpenAPI KMA ASOS Service Called");

            #region [비동기 설정]

            #endregion

            #region [데이터 수집 Layer]
            // 관측소 조회
            List<KMASiteInformation> listAWS = new List<KMASiteInformation>();
            listAWS = NpgSQLService.GetSites_FromOpenAPI_KMAASOS();

            //관측소별 데이터요청 (파일로 다운)
            if (listAWS != null)
            {
                //현재 시간 기준으로 이전 1시간 60분 데이터를 요청
                DateTime curDateTime = DateTime.Now;

                //foreach (KMASiteInformation site in listAWS)
                //{
                    string serviceURL = "https://apihub-pub.kma.go.kr/api/typ01/url/kma_sfcdd.php?";
                    string tm = curDateTime.ToString("yyyyMMdd");
                    // 지점번호 , 0이면 전체지점
                    string stn = "0";
                    //1 : 일정간격, 0 : 빈칸없는 CSV파일
                    string disp = "1";
                    //1 : Head에 도움말있음 , 0 : Head에 도움말 없음
                    string help = "0";
                    string authKey = "40cfe353913cd680317889498823f9214c0d7a09e09583a5b09291467c37af3414233051807b848caa2d0162742948c28969c222eb3dcdfc061abea91ac9d60a";

                    Uri uri = new Uri(string.Format("{0}tm={1}&stn={2}&disp={3}&help={4}&authKey={5}", serviceURL, tm, stn, disp, help, authKey));
                    string filePath = KMA_Controller.ExecuteDownloadResponse(uri, tm, stn);

                    #region [데이터 입력 Layer]
                    //데이터 들어온거 확인
                    if (filePath != string.Empty)
                    {
                        EnqueueOpenAPIKMAASOSResult(filePath);
                    }
                    #endregion
                //}
            }
            #endregion
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
                listStatus.Items.Add(string.Format("{0}-{1}", DateTime.Now, message)); // listBox1에 메시지를 추가 (예: 로그 출력)
            }
        }
        #endregion

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
