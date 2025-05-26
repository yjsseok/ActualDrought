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
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UFRI.FrameWork;


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
            InitializeLogNBuild();
            InitializeVariables();
            InitializeSites();
            if (InitializeDatabase())
            {
                WriteStatus("데이터베이스 초기화 성공");
            }
            else
            {
                WriteStatus("데이터베이스 초기화 실패");
            }
        }

        private void InitializeLogNBuild()
        {
            // 로그 설정 및 버전 표시 로직
        }

        private void InitializeVariables()
        {
            this.isServiceRunning = false;
            _global.PeriodUse = BizCommon.BoolConvert(Config.PeriodUse);
            _global.startDate = new DateTime(Config.StartDate, 1, 1);
            _global.endDate = new DateTime(Config.EndDate, 12, 31);
        }

        private void InitializeSites()
        {
            // 농업용 저수지 사이트 초기화 로직
        }

        private bool InitializeDatabase()
        {
            // 데이터베이스 연결 초기화 로직
            return true; // 임시 반환값
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isServiceRunning)
            {
                bool success = ServiceStart();
                if (success)
                {
                    WriteStatus("서비스가 성공적으로 시작되었습니다.");
                    btnStart.Enabled = false;
                    btnStop.Enabled = true;
                }
                else
                {
                    WriteStatus("서비스 시작에 실패했습니다.");
                }
            }
            else
            {
                WriteStatus("서비스가 이미 실행 중입니다.");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isServiceRunning)
            {
                ServiceStop();
                WriteStatus("서비스가 중지되었습니다.");
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            else
            {
                WriteStatus("서비스가 실행 중이 아닙니다.");
            }
        }

        private bool ServiceStart()
        {
            try
            {
                if (_global.RealTimeUse == true)
                {
                    thOpenAPI_AG_tb_reserviorlevel = new Thread(OpenAPI_AG_tb_reserviorlevel_AutoCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_AG_tb_reserviorlevel.Start();
                }

                if (_global.PeriodUse)
                {
                    thOpenAPI_AG_tb_reserviorlevel_Period = new Thread(OpenAPI_AG_tb_reserviorlevel_PeriodCaller)
                    {
                        IsBackground = true
                    };
                    thOpenAPI_AG_tb_reserviorlevel_Period.Start();
                }

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
                WriteStatus($"서비스 시작 오류: {ex.Message}");
                return false;
            }
        }
        private void OpenAPI_AG_tb_reserviorlevel_AutoCaller()
        {
            // 설정된 시간 간격으로 호출 (초 단위)
            int nTimeGap = 1000 * Config.AG_tb_reserviorlevel_Auto_Caller_Second;
            _logger.Info($"실시간 데이터 수집 간격: {Config.AG_tb_reserviorlevel_Auto_Caller_Second}초", "AutoCaller");

            deleOpenAPI_AG_tb_reserviorlevel_Caller deleMethod = new deleOpenAPI_AG_tb_reserviorlevel_Caller(this.OpenAPI_AG_tb_reserviorlevel_Service);

            while (!_shouldStop)
            {
                IAsyncResult ar = deleMethod.BeginInvoke(null, null);
                Thread.Sleep(nTimeGap);
            }
        }

        private void OpenAPI_AG_tb_reserviorlevel_Service()
        {
            try
            {
                WriteStatus("농업용 저수지 실시간 데이터 수집 모듈 시작");

                // DB에서 최종 데이터 일자 조회
                DateTime lastDate = NpgSQLService.GetLastDateFromOpenAPI_AG_tb_reserviorlevel();
                DateTime today = DateTime.Today;

                // 최종 데이터가 오늘 날짜보다 이후인 경우 작업 취소
                if (lastDate.Date >= today.Date)
                {
                    string message = string.Format("최종 데이터({0})가 오늘자입니다. 데이터 수집을 건너뜁니다.",
                        lastDate.ToString("yyyy-MM-dd"));
                    WriteStatus(message);
                    return; // 메서드 종료
                }

                // 농업용 저수지 정보 가져오기
                List<AgriDamSpec> listAgriDam = NpgSQLService.Get_AgriDamSpec();

                if (listAgriDam != null && listAgriDam.Count > 0)
                {
                    // 최종일자 다음날부터 오늘까지 한 번에 요청
                    DateTime startDate = lastDate.AddDays(1);
                    WriteStatus($"데이터 수집 시작: {startDate.ToString("yyyy-MM-dd")} ~ {today.ToString("yyyy-MM-dd")}");

                    foreach (AgriDamSpec dam in listAgriDam)
                    {
                        if (_shouldStop) break;

                        try
                        {
                            WriteStatus($"[{dam.facName}] 저수지 데이터 조회 중... ({startDate.ToString("yyyy-MM-dd")} ~ {today.ToString("yyyy-MM-dd")})");

                            // API 호출하여 데이터 가져오기 - 기간 전체를 한 번에 요청
                            List<ReservoirLevelData> data = GetReservoirDataAsync(dam.facCode, startDate, today).Result;

                            if (data != null && data.Count > 0)
                            {
                                // 결과 큐에 추가
                                lock (locker)
                                {
                                    OpenAPI_AG_tb_reserviorlevel_ResultQueue.Enqueue(data);
                                }

                                // 결과 처리 이벤트 발생
                                eventWaitHandle.Set();
                                WriteStatus($"[{dam.facName}] 저수지 데이터 {data.Count}건 처리 완료");
                            }
                            else
                            {
                                WriteStatus($"[{dam.facName}] 저수지 데이터가 없습니다.");
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteStatus($"[{dam.facName}] 저수지 데이터 처리 중 오류 발생: {ex.Message}");
                            GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                            GMLogHelper.WriteLog($"Message: {ex.Message}");
                        }

                        // API 호출 간 딜레이 추가 (과도한 요청 방지)
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    WriteStatus("저수지 정보를 가져오는데 실패했습니다.");
                }

                WriteStatus("농업용 저수지 실시간 데이터 수집 모듈 종료");
            }
            catch (Exception ex)
            {
                WriteStatus($"농업용 저수지 실시간 데이터 수집 모듈 오류: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
            }
        }
        private async void OpenAPI_AG_tb_reserviorlevel_PeriodCaller()
        {
            try
            {
                this.WriteStatus("농업용 저수지 기간 조회 모듈 시작");

                // 선택된 기간 가져오기
                DateTime startDate = _global.startDate;
                DateTime endDate = _global.endDate;
                DateTime today = DateTime.Today;

                // 종료일이 오늘 날짜를 초과하는지 확인
                if (endDate > today)
                {
                    WriteStatus("종료일은 오늘 날짜를 초과할 수 없습니다.");
                    endDate = today;
                }

                // 기간이 365일을 초과하는지 확인
                TimeSpan dateDiff = endDate - startDate;
                if (dateDiff.TotalDays > 365)
                {
                    WriteStatus("조회 기간은 최대 365일까지 가능합니다. 1년 단위로 나누어 조회합니다.");

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

                        WriteStatus($"기간 조회: {currentStartDate.ToString("yyyy-MM-dd")} ~ {currentEndDate.ToString("yyyy-MM-dd")}");

                        // 여기서 현재 기간에 대한 데이터 조회 로직 실행
                        List<AgriDamSpec> listAgriDam = NpgSQLService.Get_AgriDamSpec();
                        if (listAgriDam != null && listAgriDam.Count > 0)
                        {
                            foreach (AgriDamSpec dam in listAgriDam)
                            {
                                if (_shouldStop) break;

                                try
                                {
                                    WriteStatus($"[{dam.facName}] 저수지 데이터 조회 중... ({currentStartDate.ToString("yyyy-MM-dd")} ~ {currentEndDate.ToString("yyyy-MM-dd")})");

                                    // API 호출하여 데이터 가져오기
                                    List<ReservoirLevelData> data = await GetReservoirDataAsync(dam.facCode, currentStartDate, currentEndDate);

                                    if (data != null && data.Count > 0)
                                    {
                                        // 결과 큐에 추가
                                        lock (locker)
                                        {
                                            OpenAPI_AG_tb_reserviorlevel_ResultQueue.Enqueue(data);
                                        }

                                        // 결과 처리 이벤트 발생
                                        eventWaitHandle.Set();

                                        WriteStatus($"[{dam.facName}] 저수지 데이터 {data.Count}건 처리 완료");
                                    }
                                    else
                                    {
                                        WriteStatus($"[{dam.facName}] 저수지 데이터가 없습니다.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteStatus($"[{dam.facName}] 저수지 데이터 처리 중 오류 발생: {ex.Message}");
                                    GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                                    GMLogHelper.WriteLog($"Message: {ex.Message}");
                                }

                                // API 호출 간 딜레이 추가 (과도한 요청 방지)
                                await Task.Delay(100);
                            }
                        }
                        else
                        {
                            WriteStatus("저수지 정보를 가져오는데 실패했습니다.");
                        }

                        // 다음 시작일 설정 (현재 종료일 다음날)
                        currentStartDate = currentEndDate.AddDays(1);
                    }
                }
                else
                {
                    // 365일 이내인 경우 한 번에 조회
                    WriteStatus($"기간 조회: {startDate.ToString("yyyy-MM-dd")} ~ {endDate.ToString("yyyy-MM-dd")}");

                    // 여기서 전체 기간에 대한 데이터 조회 로직 실행
                    List<AgriDamSpec> listAgriDam = NpgSQLService.Get_AgriDamSpec();
                    if (listAgriDam != null && listAgriDam.Count > 0)
                    {
                        foreach (AgriDamSpec dam in listAgriDam)
                        {
                            if (_shouldStop) break;

                            try
                            {
                                WriteStatus($"[{dam.facName}] 저수지 데이터 조회 중... ({startDate.ToString("yyyy-MM-dd")} ~ {endDate.ToString("yyyy-MM-dd")})");

                                // API 호출하여 데이터 가져오기
                                List<ReservoirLevelData> data = await GetReservoirDataAsync(dam.facCode, startDate, endDate);

                                if (data != null && data.Count > 0)
                                {
                                    // 결과 큐에 추가
                                    lock (locker)
                                    {
                                        OpenAPI_AG_tb_reserviorlevel_ResultQueue.Enqueue(data);
                                    }

                                    // 결과 처리 이벤트 발생
                                    eventWaitHandle.Set();

                                    WriteStatus($"[{dam.facName}] 저수지 데이터 {data.Count}건 처리 완료");
                                }
                                else
                                {
                                    WriteStatus($"[{dam.facName}] 저수지 데이터가 없습니다.");
                                }                          
                            }
                            catch (Exception ex)
                            {
                                WriteStatus($"[{dam.facName}] 저수지 데이터 처리 중 오류 발생: {ex.Message}");
                                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                                GMLogHelper.WriteLog($"Message: {ex.Message}");
                            }

                            // API 호출 간 딜레이 추가 (과도한 요청 방지)
                            await Task.Delay(100);
                        }
                    }
                    else
                    {
                        WriteStatus("저수지 정보를 가져오는데 실패했습니다.");
                    }
                }

                this.WriteStatus("농업용 저수지 기간 조회 모듈 종료");
            }
            catch (Exception ex)
            {
                WriteStatus($"농업용 저수지 기간 조회 모듈 오류: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
            }
        }
       
        private async Task<List<ReservoirLevelData>> GetReservoirDataAsync(string damCode, DateTime startDate, DateTime endDate)
        {
            List<ReservoirLevelData> result = new List<ReservoirLevelData>();
            try
            {
                string formattedStartDate = startDate.ToString("yyyyMMdd");
                string formattedEndDate = endDate.ToString("yyyyMMdd");

                // API URL 및 키 설정
                string apiUrl = "http://apis.data.go.kr/B552149/reserviorWaterLevel/reservoirlevel/";
        //        string apiKey = "FpAShNYZTSjw5iNsUwVK867BWOExI9aW6YstOhSMmgEEquLAatpmvK9ZvuqaKJsKY%2BVAuuSlChy%2BP2xhEYDq6g%3D%3D";
           
                string apiKey = "twmcFC573zbkqrRUA%2BFaZiry4YSubsQGruB02GpMgc%2BMjbR8NGIKMR8yBPMzpIjwvTajJYsn3OJkb0DF6ERunw%3D%3D";
        //        string apiKey = "TSuYnoFvXeiYo14wN2fk8Kyk%2F5jNUyPZ%2F47AM89XIslpdAR%2FMc1OwpiCsYILkD7mSSDfVUPQxGWYXofSuuXPPw%3D%3D";
        //        string apiKey = "TeSSIf1TYsuPoXt3gW4TDbqjfzc % 2BkSCD3bFhjHgfPzK9JGkaRBHVSRSIM378w % 2Fgi7d9tJ28xK4dx7lgdUqAgug % 3D % 3D";
        //        string apiKey = "Dmja4F % 2FdbdoCx8oq8ys4irj7IOSs6xYv3Ac3no31WAwWyMc % 2F0Gs25VFDG7NKNviyhGK24do % 2F % 2BeH5bBlMwDEj % 2Bw % 3D % 3D";



              // pageNo=1&numOfRows=1000 파라미터 추가
              string requestUrl = $"{apiUrl}?serviceKey={apiKey}&pageNo=1&numOfRows=1000&fac_code={damCode}&date_s={formattedStartDate}&date_e={formattedEndDate}";

                _logger.Debug($"API 요청 URL: {requestUrl}", "API");

                // HttpClient 생성
                using (HttpClient httpClient = new HttpClient())
                {
                    // 응답을 바이트 배열로 받아서 처리
                    HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                    string xmlResponse = Encoding.UTF8.GetString(byteArray);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlResponse);

                    // 데이터 항목 추출
                    XmlNodeList itemNodes = xmlDoc.SelectNodes("//item");
                    if (itemNodes != null && itemNodes.Count > 0)
                    {
                        foreach (XmlNode item in itemNodes)
                        {
                            var data = new ReservoirLevelData
                            {
                                check_date = GetNodeValue(item, "check_date"),
                                county = GetNodeValue(item, "county"),
                                fac_code = GetNodeValue(item, "fac_code"),
                                fac_name = GetNodeValue(item, "fac_name"),
                                rate = GetNodeValue(item, "rate"),
                            };
                            result.Add(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteStatus($"API 호출 오류 (저수지 코드: {damCode}): {ex.Message}");
            }

            return result;
        }

        private string GetNodeValue(XmlNode parentNode, string nodeName)
        {
            XmlNode node = parentNode.SelectSingleNode(nodeName);
            return node != null ? node.InnerText : string.Empty;
        }

        private void OpenAPI_AG_tb_reserviorlevel_ResultCaller()
        {
            WriteStatus("농업용 저수지 결과 처리 모듈 시작");
            while (!_shouldStop)
            {
                List<ReservoirLevelData> resultList = new List<ReservoirLevelData>();
                lock (locker)
                {
                    if (OpenAPI_AG_tb_reserviorlevel_ResultQueue.Count > 0)
                    {
                        resultList = OpenAPI_AG_tb_reserviorlevel_ResultQueue.Dequeue();
                    }
                }

                if (resultList.Count > 0)
                {
                    // 결과 저장 로직 구현
                    SaveReservoirDataToDatabase(resultList);
                }
                else
                {
                    eventWaitHandle.WaitOne(100);
                }
            }
            WriteStatus("농업용 저수지 결과 처리 모듈 종료");
        }

        private bool SaveReservoirDataToDatabase(List<ReservoirLevelData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return false;

            try
            {
                // 기존 데이터 확인 (중복 방지)
                string facCode = dataList.First().fac_code;
                string startDate = dataList.Min(d => d.check_date);
                string endDate = dataList.Max(d => d.check_date);

                // 기존 데이터 조회
                List<ReservoirLevelData> existingData = NpgSQLService.GetReservoirLevelData(facCode, startDate, endDate);

                // 중복되지 않은 데이터만 필터링
                List<ReservoirLevelData> newData = dataList.Where(current =>
                    !existingData.Any(db =>
                        db.check_date == current.check_date &&
                        db.fac_code == current.fac_code)
                ).ToList();

                if (newData.Count > 0)
                {
                    // 벌크 삽입 실행
                    WriteStatus($"데이터베이스에 {newData.Count}건 저장 중...");
                    bool result = NpgSQLService.BulkInsert_ReservoirLevelData(newData);

                    if (result)
                    {
                        WriteStatus($"데이터베이스에 {newData.Count}건 저장 완료");
                        return true;
                    }
                    else
                    {
                        WriteStatus("데이터베이스 저장 실패");
                        return false;
                    }
                }
                else
                {
                    WriteStatus("저장할 새로운 데이터가 없습니다.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                WriteStatus($"DB 저장 오류: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
                return false;
            }
        }
        private void ServiceStop()
        {
            try
            {
                _shouldStop = true;
                eventWaitHandle.Set();

                if (thOpenAPI_AG_tb_reserviorlevel != null && thOpenAPI_AG_tb_reserviorlevel.IsAlive)
                    thOpenAPI_AG_tb_reserviorlevel.Join(100);

                if (thOpenAPI_AG_tb_reserviorlevel_Period != null && thOpenAPI_AG_tb_reserviorlevel_Period.IsAlive)
                    thOpenAPI_AG_tb_reserviorlevel_Period.Join(100);

                if (thOpenAPI_AG_tb_reserviorlevel_Result != null && thOpenAPI_AG_tb_reserviorlevel_Result.IsAlive)
                    thOpenAPI_AG_tb_reserviorlevel_Result.Join(100);

                isServiceRunning = false;
            }
            catch (Exception ex)
            {
                WriteStatus($"서비스 중지 오류: {ex.Message}");
            }
        }

        private void WriteStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteStatus(message)));
            }
            else
            {
                listStatus.Items.Add($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}");
                listStatus.SelectedIndex = listStatus.Items.Count - 1;
                listStatus.Refresh();
            }
        }

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
    }
}
        #endregion