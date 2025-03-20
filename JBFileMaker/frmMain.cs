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
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace JBFileMaker
{
    public partial class frmMain : Form
    {
        public List<MatchingTable> listMatching = new List<MatchingTable>();
        public List<DamInfo> listDamInfo = new List<DamInfo>();

        public DateTime startDate = new DateTime(1989, 1, 1);
        public DateTime endDate = new DateTime(2024, 12, 31);

        public frmMain()
        {
            InitializeComponent();
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

        private void btnMet_Click(object sender, EventArgs e)
        {
            List<string> sggCodes = new List<string>();
            sggCodes = NpgSQLService.GetSggCD_FromOpenAPI_KMAASOS();
            WriteStatus(string.Format("Read SggCode : {0}", sggCodes.Count));

            List<ASOSThiessen> listThiessen = new List<ASOSThiessen>();
            listThiessen = NpgSQLService.GetThiessen_FromOpenAPI_KMAASOS();
            WriteStatus(string.Format("Read Thiessen : {0}", listThiessen.Count));

            //AI 디렉토리 확인
            string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export", "MI");
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (string sggcd in sggCodes)
            {
                //해당 관측지 추출
                List<ASOSThiessen> selectedThiessen = new List<ASOSThiessen>();
                selectedThiessen = listThiessen.Where(a=>a.sgg_cd == sggcd).ToList();

                //시군구 코드별 면적강우 저장
                AreaRainfall sgg_AreaRainfall = new AreaRainfall();
                sgg_AreaRainfall.sggCode = sggcd;

                //합산 1확인
                if (ThiessenValidate(selectedThiessen) == true)
                {
                    if (selectedThiessen.Count > 0)
                    {
                        List<PointRainfall> dailyDatabySite = new List<PointRainfall>();

                        foreach (ASOSThiessen thiessen in selectedThiessen)
                        {
                            PointRainfall pData = NpgSQLService.GetDailyRainfall_FromOpenAPI_KMAASOS(int.Parse(thiessen.code), this.startDate, this.endDate);
                            pData.ratio = double.Parse(thiessen.ratio);
                            dailyDatabySite.Add(pData);
                        }

                        //면적강우
                        if (CountValidate(dailyDatabySite) == true)
                        {
                            WriteStatus(string.Format("지점별 강우 데이터 개수 동일함"));
                            sgg_AreaRainfall.CollectionPointRainfall.AddRange(dailyDatabySite);
                            sgg_AreaRainfall.listAreaRainfall = CalculateAreaRainfall_SameDataCnt(dailyDatabySite);                            
                        }
                        else
                        {
                            WriteStatus(string.Format("지점별 강우 데이터 개수 상이함"));
                            List<tsTimeSeries> listAreaRainfall = new List<tsTimeSeries>();
                            listAreaRainfall = CalculateAreaRainfall_SameDataCnt(dailyDatabySite);
                        }
                    }

                    //시군구코드명로 파일 만들기
                    BizFileIO.WriteMIData(targetDir, sggcd, sgg_AreaRainfall);

                    //년,월,일,JD,강우량,관측소코드...
                    //yyyy,MM,dd,day(일수),면적강우,관측소별강우 (n개)
                }
                else
                {
                    WriteStatus(string.Format("Thiessen Error : {0}", sggcd));
                }
            }
        }

        private List<tsTimeSeries> CalculateAreaRainfall_SameDataCnt(List<PointRainfall> dailyDatabySite)
        {
            List<tsTimeSeries> listAreaRainfall = new List<tsTimeSeries>();

            PointRainfall std = dailyDatabySite.First();

            int i = 0;
            foreach (var item in std.listRainfall)
            {
                tsTimeSeries rn = new tsTimeSeries();

                rn.tm = item.tm;
                rn.tmdt = item.tmdt;
                //rn.DayOfYear = item.tmdt.DayOfYear;

                rn.rainfall = CalculateAreaRainfall(dailyDatabySite, i);

                //윤달제거
                if (rn.tmdt.Month == 2 && rn.tmdt.Day == 29)
                {

                }
                else
                {
                    listAreaRainfall.Add(rn);
                }
                
                i++;
            }

            return listAreaRainfall;
        }

        private double CalculateAreaRainfall(List<PointRainfall> dailyDatabySite, int i)
        {
            double areaRainfall = 0.0;
            double sumRatio = 0.0;

            foreach (PointRainfall point in dailyDatabySite)
            {
                sumRatio += point.ratio;
                double rainfall = (point.listRainfall[i].rainfall < 0) ? 0 : point.listRainfall[i].rainfall;
                areaRainfall += point.ratio * rainfall;
            }

            if (sumRatio == 1)
            {
                return areaRainfall;
            }
            else
            {
                return -9999;
            }
        }

        private void CalculateAreaRainfall_DifferentCnt(List<PointRainfall> dailyDatabySite)
        {
            throw new NotImplementedException();
        }

        private bool CountValidate(List<PointRainfall> dailyDatabySite)
        {
            //평균
            double avg = dailyDatabySite.Average(a => a.listRainfall.Count);
            double cnt = dailyDatabySite.First().listRainfall.Count;

            if (avg == cnt)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ThiessenValidate(List<ASOSThiessen> selectedThiessen)
        {
            double sumRatio = selectedThiessen.Select(x => double.TryParse(x.ratio, out double value) ? value : 0.0).Sum();

            if (sumRatio == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void btnAgri_Click(object sender, EventArgs e)
        {
            //전체 시군구코드

            //전체 농업용저수지
            //시군구코드 (농업용저수지 앞 5자리)

            //AI 만들고
            //시군구코드 디렉토리
            //농업용저수지코드.csv

            //년,월,일,JD,저수율,저수위
            //yyyy,MM,dd,day(일수),rate,level



        }

        private void btnMakeFile_Click(object sender, EventArgs e)
        {
            //MatchingTable 읽기
            string filePath = Path.Combine(Application.StartupPath, "Files", "CodeMatching.csv");
            this.listMatching = BizFileIO.ReadCSV<MatchingTable>(filePath);
            WriteStatus(string.Format("CodeMatching Read : {0}", this.listMatching.Count));

            //댐정보 읽기
            string damFilePath = Path.Combine(Application.StartupPath, "Files", "DamInfo.csv");
            this.listDamInfo = BizFileIO.ReadCSV<DamInfo>(damFilePath);
            WriteStatus(string.Format("DamInfo Read : {0}", this.listDamInfo.Count));

            //디렉토리 확인
            string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export", "HI");
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            //파일 생성 시작
            if (listMatching.Count > 0)
            {
                foreach (MatchingTable item in this.listMatching)
                {
                    //DataType별로 파일을 생성
                    if (item.DataType == "dam")
                    {
                        DamDataSearch(item);
                    }
                    else if (item.DataType == "flow")
                    {
                        //유량자료 조회
                        FlowDataSearch(item);
                    }
                    else if (item.DataType == "AgriDam")
                    {
                        //농업용저수지 조회
                        AgriDamDataSearch(item);
                    }
                }
            }
        }

        private void AgriDamDataSearch(MatchingTable item)
        {
            throw new NotImplementedException();
        }

        private void FlowDataSearch(MatchingTable item)
        {
            throw new NotImplementedException();
        }

        private void DamDataSearch(MatchingTable item)
        {
            //댐자료 조회
            var correspondingDam = listDamInfo.FirstOrDefault(d => d.damID == item.MatchKey);

            if (correspondingDam != null)
            {
                List<DamHRData> listDamDatas = new List<DamHRData>();
                listDamDatas = NpgSQLService.GetDailyDatasFromOpenAPIWAMISDamHrData_TimeCorrection(correspondingDam.damCode);


                //시간단위 자료이기 때문에 일단위 자료로 변환하는 모듈
                //시작일 부터 마지막일까지 순환하면서 데이터를 추출
                if (listDamDatas.Count > 0)
                {
                    DateTime startDate = BizCommon.StringtoDateTime_24Correction(listDamDatas.First().obsdh.Trim(), "yyyyMMddHH");
                    DateTime endDate = BizCommon.StringtoDateTime_24Correction(listDamDatas.Last().obsdh.Trim(), "yyyyMMddHH");

                    var filteredData = listDamDatas
                        .GroupBy(d => d.GetObservationDateTime().ToString("yyyyMMdd")) // 날짜 기준 그룹화
                        .Select(g => g.OrderBy(d => d.GetObservationDateTime()) // 시간순 정렬
                                      .FirstOrDefault(d => d.GetRwlAsDouble() > 0)) // 첫 번째 유효한 데이터 선택
                        .Where(d => d != null) // null 제외
                        .OrderBy(d => d.GetObservationDateTime()) // 최종 정렬
                        .ToList();

                    //HI 디렉토리 생성
                    //파일생성
                    string filePath = Path.Combine(Application.StartupPath, "HI", string.Format("{0}.csv", item.RegionCode.Trim()));

                    using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Default))
                    {
                        //Dam 저수량
                        //Flow 유량
                        //AgriDam rate

                        //시군구코드명로 파일 만들기

                        //년,월,일,JD,값
                        //yyyy,MM,dd,day(일수), value
                        

                    }

                    foreach (var data in filteredData)
                    {
                        int kkk = 0;
                    }
                }

            }
            else
            {
                this.WriteStatus(string.Format("Error : {0}-{1}-{2} 데이터 없음 !", item.RegionCode, item.DataType, item.MatchKey));
            }
        }

        #region [WriteStatus]
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
                GMLogHelper.WriteLog(message);
            }
        }

        #endregion

        
    }
}
