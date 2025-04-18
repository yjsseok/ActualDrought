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

        #region [Varibales]
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
            {  }
            else
            {  }
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

            // frmConfig의 DTP에서 날짜 범위 가져오기
            frmConfig configForm = new frmConfig();
            _global.startDate = configForm.dtpStart.Value;
            _global.endDate = configForm.dtpEnd.Value;
        }

        private void InitializeSites()
        {
            WamisAPIController apiController = new WamisAPIController();
            DataTable dt = new DataTable();
            dt = apiController.GetObsData("fl_obs");

            List<FlowSiteInformation> listObs = new List<FlowSiteInformation>();
            listObs = BizCommon.ConvertDataTableToList<FlowSiteInformation>(dt);
            _global.listFlowOBS = listObs;

            this.WriteStatus(string.Format("초기화 {0}개의 유량관측지점", listObs.Count));
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
                #region [WAMIS Flow 자료수집 구동]
                //임시로 리얼타임 돌리지 않음
               // _global.RealTimeUse = true;

                if (_global.RealTimeUse == true)
                {
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
                    thOpenAPI_WAMIS_Period = new Thread(OpenAPI_WAMIS_Flow_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_WAMIS_Period.Start();
                }
                #endregion

                #region [WAMIS Flow 결과처리]
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
                // 실시간 데이터 수집 스레드 종료
                if (thOpenAPI_WAMIS_Flow != null && thOpenAPI_WAMIS_Flow.IsAlive)
                {
                    thOpenAPI_WAMIS_Flow.Abort();
                }

                // 기간별 데이터 수집 스레드 종료
                if (thOpenAPI_WAMIS_Period != null && thOpenAPI_WAMIS_Period.IsAlive)
                {
                    thOpenAPI_WAMIS_Period.Abort();
                }

                // 결과 처리 스레드 종료
                if (thOpenAPI_WAMIS_Result != null && thOpenAPI_WAMIS_Result.IsAlive)
                {
                    thOpenAPI_WAMIS_Result.Abort();
                }

                isServiceRunning = false; // 서비스 실행 상태 해제
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("Error stopping service: {0}", ex.Message));
            }
        }


        private void OpenAPI_WAMIS_Flow_AutoCaller() /////JS
        {
            //구동시설 설정
            int nTimeGap = 1000 * Config.WAMIS_Flow_Auto_Caller_Second;
            deleWAMISFlow_AutoCaller dele_WAMIS_Flow = new deleWAMISFlow_AutoCaller(OpenAPI_WAMIS_Flow_Service);

            while (true)
            {
                IAsyncResult ar = dele_WAMIS_Flow.BeginInvoke(null, null);
                Thread.Sleep(nTimeGap);

            }
        }


        /*백업1
        private async void OpenAPI_WAMIS_Flow_Service()
        {
            try
            {
                this.WriteStatus("WAMIS Flow Module Start");
                string serviceURL = "http://www.wamis.go.kr:8080/wamis/openapi/wkw/flw_dtdata";
                string authKey = "b4568bbc61dabc1ce232c94d538f9f7d45229c1620";

                foreach (FlowSiteInformation obs in _global.listFlowOBS)
                {
                    this.WriteStatus($"{obs.obsnm} 관측소 데이터 수집 시작");
                    int year = DateTime.Now.Year;
                    string parameters = $"?obscd={obs.obscd}&year={year}&authKey={authKey}";
                    Uri uri = new Uri(serviceURL + parameters);

                    List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, year);
                    if (flowData != null && flowData.Count > 0)
                    {
                        this.WriteStatus($"수집된 {obs.obsnm} 관측소 데이터: {flowData.Count}개");
                        EnqueueOpenAPIWAMISFlowResult(flowData);
                    }
                    else
                    {
                        this.WriteStatus($"{obs.obsnm} 관측소 데이터 없음");
                    }
                    Thread.Sleep(1000);
                }
                this.WriteStatus("WAMIS Flow Module End");
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
                this.WriteStatus($"WAMIS Flow Module Error: {ex.Message}");
            }
        }

        
        private async void OpenAPI_WAMIS_Flow_PeriodCaller()
        {
            foreach (FlowSiteInformation obs in _global.listFlowOBS)
            {
                for (int i = obs.minYear; i <= obs.maxYear; i++)
                {
                    List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, i);

                    if (flowData != null)
                    {
                        // 365일치 데이터 생성 및 List에 저장
                        List<FlowData> dailyFlowDataList = new List<FlowData>();
                        DateTime startDate = new DateTime(i, 1, 1);

                        int TotalDays = BizCommon.GetTotalDays(i);

                        for (int j = 0; j < TotalDays; j++)
                        {
                            string ymd = startDate.AddDays(j).ToString("yyyyMMdd");
                            FlowData dailyFlowData = new FlowData { obscd = obs.obscd, ymd = ymd, flw = double.NaN }; // 기본값으로 NaN 설정
                            if (flowData.Any(data => data.ymd == ymd))
                            {
                                dailyFlowData.flw = flowData.FirstOrDefault(data => data.ymd == ymd).flw;
                            }
                            dailyFlowDataList.Add(dailyFlowData);
                        }

                        //DataQueue에 넣기
                        if (dailyFlowDataList.Count > 0)
                        {
                            EnqueueOpenAPIWAMISFlowResult(dailyFlowDataList);
                        }
                    }
                }
            }
        }*/
        private async void OpenAPI_WAMIS_Flow_Service()
        {
            try
            {
                this.WriteStatus("WAMIS Flow Module Start");

                // DB에서 최종 데이터 일자 조회
                DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_WAMIS_Flow();
                DateTime today = DateTime.Today;

                // 최종 데이터가 오늘 날짜보다 이후인 경우 작업 취소
                if (lastDate.Date >= today.Date)
                {
                    WriteStatus(string.Format("최종 데이터({0})가 오늘자입니다. 데이터 수집을 건너뜁니다.",
                        lastDate.ToString("yyyy-MM-dd"), today.ToString("yyyy-MM-dd")));
                    return; // 메서드 종료
                }

                string serviceURL = "http://www.wamis.go.kr:8080/wamis/openapi/wkw/flw_dtdata";
                string authKey = "b4568bbc61dabc1ce232c94d538f9f7d45229c1620";
                foreach (FlowSiteInformation obs in _global.listFlowOBS)
                {
                    this.WriteStatus($"{obs.obsnm} 관측소 데이터 수집 시작");
                    int year = DateTime.Now.Year;
                    string parameters = $"?obscd={obs.obscd}&year={year}&authKey={authKey}";
                    Uri uri = new Uri(serviceURL + parameters);
                    List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, year);

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

                        this.WriteStatus($"수집된 {obs.obsnm} 관측소 데이터: {filteredData.Count}개 (필터링 전: {flowData.Count}개)");

                        if (filteredData.Count > 0)
                        {
                            EnqueueOpenAPIWAMISFlowResult(filteredData);
                        }
                    }
                    else
                    {
                        this.WriteStatus($"{obs.obsnm} 관측소 데이터 없음");
                    }
                    Thread.Sleep(100);
                }
                this.WriteStatus("WAMIS Flow Module End");
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
                this.WriteStatus($"WAMIS Flow Module Error: {ex.Message}");
            }
        }
        /*
        private async void OpenAPI_WAMIS_Flow_PeriodCaller()
        {
            foreach (FlowSiteInformation obs in _global.listFlowOBS)
            {
                for (int i = obs.minYear; i <= obs.maxYear; i++)
                {
                    List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, i);
                    if (flowData != null)
                    {
                        // 데이터 생성 및 List에 저장
                        List<FlowData> dailyFlowDataList = new List<FlowData>();
                        DateTime startDate = new DateTime(i, 1, 1);

                        // 현재 연도인 경우 오늘까지만 처리
                        DateTime endDate;
                        if (i == DateTime.Now.Year)
                        {
                            endDate = DateTime.Now;
                            this.WriteStatus($"{obs.obsnm} 관측소 {i}년 데이터 - 현재 날짜({endDate.ToString("yyyy-MM-dd")})까지만 처리");
                        }
                        else
                        {
                            endDate = new DateTime(i, 12, 31);
                            this.WriteStatus($"{obs.obsnm} 관측소 {i}년 데이터 - 전체 처리");
                        }

                        int totalDays = (endDate - startDate).Days + 1;

                        for (int j = 0; j < totalDays; j++)
                        {
                            DateTime currentDate = startDate.AddDays(j);
                            string ymd = currentDate.ToString("yyyyMMdd");

                            FlowData dailyFlowData = new FlowData { obscd = obs.obscd, ymd = ymd, flw = double.NaN }; // 기본값으로 NaN 설정
                            if (flowData.Any(data => data.ymd == ymd))
                                dailyFlowData.flw = flowData.FirstOrDefault(data => data.ymd == ymd).flw;

                            dailyFlowDataList.Add(dailyFlowData);
                        }

                        //DataQueue에 넣기
                        if (dailyFlowDataList.Count > 0)
                        {
                            this.WriteStatus($"{obs.obsnm} 관측소 {i}년 처리 데이터 수: {dailyFlowDataList.Count}개");
                            EnqueueOpenAPIWAMISFlowResult(dailyFlowDataList);
                        }
                    }
                }
            }
        }*/
        private async void OpenAPI_WAMIS_Flow_PeriodCaller()
        {
            foreach (FlowSiteInformation obs in _global.listFlowOBS)
            {
                // 설정된 기간에서 연도 범위 추출
                int startYear = _global.startDate.Year;
                int endYear = _global.endDate.Year;

                // 설정된 기간과 관측소의 연도 범위 중 겹치는 부분만 처리
                int processStartYear = Math.Max(obs.minYear, startYear);
                int processEndYear = Math.Min(obs.maxYear, endYear);

                for (int i = processStartYear; i <= processEndYear; i++)
                {
                    List<FlowData> flowData = await WAMIS_Controller.GetFlowDataAsync(obs.obscd, i);
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
                            this.WriteStatus($"{obs.obsnm} 관측소 {i}년 데이터 - 현재 날짜({periodEndDate.ToString("yyyy-MM-dd")})까지만 처리");
                        }
                        else
                        {
                            this.WriteStatus($"{obs.obsnm} 관측소 {i}년 데이터 - {periodStartDate.ToString("yyyy-MM-dd")}부터 {periodEndDate.ToString("yyyy-MM-dd")}까지 처리");
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

                        //DataQueue에 넣기
                        if (dailyFlowDataList.Count > 0)
                        {
                            this.WriteStatus($"{obs.obsnm} 관측소 {i}년 처리 데이터 수: {dailyFlowDataList.Count}개");
                            EnqueueOpenAPIWAMISFlowResult(dailyFlowDataList);
                        }
                    }
                }
            }
        }

        private void EnqueueOpenAPIWAMISFlowResult(List<FlowData> dailyFlowDataList)
        {
            //결과 파일명 Queue에 넣기
            lock (locker) OpenAPI_WAMIS_Flow_ResultQueue.Enqueue(dailyFlowDataList);

            //결과 처리하도록 이벤트 발생
            eventWaitHandle.Set();
        }

        private void OpenAPI_WAMIS_Flow_ResultCaller()
        {
            WriteStatus(string.Format("WAMIS Flow Result Caller 실행"));

            while (true)
            {
                List<FlowData> resultList = new List<FlowData>();

                lock (locker)
                {
                    if (OpenAPI_WAMIS_Flow_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_WAMIS_Flow_ResultQueue.Dequeue();
                        // If file name is null then stop worker thread
                        if (resultList.Count == 0) return;
                    }
                }

                if (resultList.Count > 0)
                {
                    //결과 저장 로직
                    OpenAPIWAMISFlowResultInsertProcess(resultList);
                }
                else
                {
                    //파일명 없을때 Wait Signal
                    eventWaitHandle.WaitOne();
                }
            }
        }

        private void OpenAPIWAMISFlowResultInsertProcess(List<FlowData> resultList)
        {
            string sDate = resultList.First().ymd;
            string eDate = resultList.Last().ymd;
            string obsCD = resultList.First().obscd;

            List<FlowData> flowData_DB = new List<FlowData>();
            flowData_DB = NpgSQLService.GetDailyDatasFromOpenAPIWAMISFlow(obsCD, sDate, eDate);

            //Database와 비교하여 Database에 없는것 입력
            List<FlowData> addDatas = resultList.Where(current => !flowData_DB.Any(db =>db.ymd == current.ymd && db.obscd == current.obscd)).ToList();

            //Bulk Insert 실행
            WriteStatus(string.Format("Insert process...{0} ea", addDatas.Count));

            if (NpgSQLService.BulkInsert_WAMISFlowDatas(addDatas) == true)
            {
                WriteStatus(string.Format("Database Insert... {0}:site {1}:Date {2}:Datas Success", obsCD, sDate.Substring(0, 4), addDatas.Count));
            }
            else
            {
                WriteStatus(string.Format("Database Insert... {0}:site {1}:Datas fail", obsCD, addDatas.Count));
            }
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
    }
}
