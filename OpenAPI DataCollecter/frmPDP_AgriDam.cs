using OpenAPI.DataServices;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UFRI.FrameWork;
using System.Threading;

namespace OpenAPI_DataCollecter
{
    public partial class frmPDP_AgriDam : Form
    {
        public Global _global { get; set; }
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiUrl = "http://apis.data.go.kr/B552149/reserviorWaterLevel/reservoirlevel/";
        private readonly string _apiKey = "FpAShNYZTSjw5iNsUwVK867BWOExI9aW6YstOhSMmgEEquLAatpmvK9ZvuqaKJsKY%2BVAuuSlChy%2BP2xhEYDq6g%3D%3D"; 
        private CancellationTokenSource _cts;
        private bool _isProcessing = false;



        #region [Delegate]
        delegate void WriteToStatusCallback(string message); // 스레드교착
        #endregion

        public frmPDP_AgriDam()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmPDP_AgriDam_Load(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.dtpStart.Value = new DateTime(1992, 01, 01);
            this.dtpEnd.Value = new DateTime(2024, 12, 31);
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnSearch":
                    Search_AgriDamData();
                    break;
            }
        }

        private async void Search_AgriDamData()
        {
            if (_isProcessing)
            {
                WriteToStatus("이미 처리 중입니다. 완료될 때까지 기다려주세요.");
                return;
            }

            try
            {
                _isProcessing = true;
                _cts = new CancellationTokenSource();

                // 상태 초기화
                listStatus.Items.Clear();
                WriteToStatus("데이터 조회 시작...");

                // 농업용저수지 데이터 조회
                List<AgriDamSpec> listAgriDam = new List<AgriDamSpec>();
                listAgriDam = NpgSQLService.Get_AgriDamSpec();

                if (listAgriDam == null || listAgriDam.Count == 0)
                {
                    WriteToStatus("저수지 정보를 가져오는데 실패했습니다.");
                    _isProcessing = false;
                    return;
                }

                WriteToStatus($"총 {listAgriDam.Count}개의 저수지 정보를 가져왔습니다.");

                // 선택된 기간 가져오기
                DateTime startDate = dtpStart.Value;
                DateTime endDate = dtpEnd.Value;
                DateTime today = DateTime.Today;

                // 종료일이 오늘 날짜를 초과하는지 확인
                if (endDate > today)
                {
                    MessageBox.Show("종료일은 오늘 날짜를 초과할 수 없습니다.", "날짜 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    endDate = today;
                    dtpEnd.Value = today;
                }

                // 기간이 365일을 초과하는지 확인
                TimeSpan dateDiff = endDate - startDate;
                if (dateDiff.TotalDays > 365)
                {
                    MessageBox.Show("조회 기간은 최대 365일까지 가능합니다.", "기간 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    endDate = startDate.AddDays(365);

                    // 수정된 종료일이 오늘 날짜를 초과하는지 다시 확인
                    if (endDate > today)
                    {
                        endDate = today;
                    }

                    dtpEnd.Value = endDate;
                }

                WriteToStatus($"조회 기간: {startDate.ToString("yyyy-MM-dd")} ~ {endDate.ToString("yyyy-MM-dd")}");

                // 저수지별로 데이터 조회 및 저장
                int successCount = 0;
                int failCount = 0;
                int totalDataCount = 0;

                foreach (AgriDamSpec dam in listAgriDam)
                {
                    if (_cts.IsCancellationRequested)
                        break;

                    try
                    {
                        WriteToStatus($"[{dam.facName}] 저수지 데이터 조회 중...");

                        // API 호출하여 데이터 가져오기
                        List<ReservoirLevelData> data = await GetReservoirDataAsync(dam.facCode, startDate, endDate);

                        if (data != null && data.Count > 0)
                        {
                            // DB에 데이터 저장
                            bool result = await SaveReservoirDataToDatabaseAsync(data);

                            if (result)
                            {
                                successCount++;
                                totalDataCount += data.Count;
                                WriteToStatus($"[{dam.facName}] 저수지 데이터 {data.Count}건 저장 완료");
                            }
                            else
                            {
                                failCount++;
                                WriteToStatus($"[{dam.facName}] 저수지 데이터 저장 실패");
                            }
                        }
                        else
                        {
                            WriteToStatus($"[{dam.facName}] 저수지 데이터가 없습니다.");
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        WriteToStatus($"[{dam.facName}] 저수지 데이터 처리 중 오류 발생: {ex.Message}");
                        GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                        GMLogHelper.WriteLog($"Message: {ex.Message}");
                    }

                    // API 호출 간 딜레이 추가 (과도한 요청 방지)
                    await Task.Delay(100);
                }

                WriteToStatus($"작업 완료: 성공 {successCount}개, 실패 {failCount}개, 총 {totalDataCount}건의 데이터 처리됨");
            }
            catch (Exception ex)
            {
                WriteToStatus($"오류 발생: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }






        private async Task<List<ReservoirLevelData>> GetReservoirDataAsync(string damCode, DateTime startDate, DateTime endDate)
        {
            List<ReservoirLevelData> result = new List<ReservoirLevelData>();

            try
            {
                string formattedStartDate = startDate.ToString("yyyyMMdd");
                string formattedEndDate = endDate.ToString("yyyyMMdd");

                // pageNo=1&numOfRows=1000 파라미터 추가
                string requestUrl = $"{_apiUrl}?serviceKey={_apiKey}&pageNo=1&numOfRows=1000&fac_code={damCode}&date_s={formattedStartDate}&date_e={formattedEndDate}";

                // 응답을 바이트 배열로 받아서 처리
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
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
            catch (Exception ex)
            {
                WriteToStatus($"API 호출 오류 (저수지 코드: {damCode}): {ex.Message}");
            }

            return result;
        }

        private string GetNodeValue(XmlNode parentNode, string nodeName)
        {
            XmlNode node = parentNode.SelectSingleNode(nodeName);
            return node != null ? node.InnerText : string.Empty;
        }

        private async Task<bool> SaveReservoirDataToDatabaseAsync(List<ReservoirLevelData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return false;

            try
            {
                // Task.Run의 결과를 반환하도록 수정
                return await Task.Run(() => NpgSQLService.BulkInsert_ReservoirLevelData(dataList));
            }
            catch (Exception ex)
            {
                WriteToStatus($"DB 저장 오류: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message: {ex.Message}");
                return false;
            }
        }

        private void WriteToStatus(string message)
        {
            if (this.InvokeRequired)
            {
                WriteToStatusCallback callback = new WriteToStatusCallback(WriteToStatus);
                this.Invoke(callback, new object[] { message });
            }
            else
            {
                listStatus.Items.Add($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}");
                listStatus.SelectedIndex = listStatus.Items.Count - 1;
                listStatus.Refresh();
            }
        }
    }
}