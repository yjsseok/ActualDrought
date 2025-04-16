using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UFRI.FrameWork;

namespace OpenAPI.Controls
{
    public class KMA_Controller
    {
        public static string ExecuteDownloadResponse(Uri baseUrl, string tm, string stn)
        {
            try
            {
                string fileName = string.Format("ASOSday_{0}_{1}.csv", stn, tm);
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Download", fileName);

                using (WebClient client = new WebClient())
                { // WebClient 인스턴스 생성
                    client.DownloadFile(baseUrl, filePath); // URL에서 파일 다운로드
                }

                return filePath;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                GMLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                return string.Empty;
            }
        }

        public static List<rcvKMAASOSData> FiletoList_KMAASOS(string filepath)
        {
            List<rcvKMAASOSData> listKMAASOSData = new List<rcvKMAASOSData>();
            try
            {
                GMLogHelper.WriteLog($"파일 읽기 시작: {filepath}");
                
                if (!File.Exists(filepath))
                {
                    GMLogHelper.WriteLog($"오류: 파일이 존재하지 않습니다 - {filepath}");
                    return null;
                }

                string[] lines = File.ReadAllLines(filepath);
                GMLogHelper.WriteLog($"파일에서 {lines.Length}줄을 읽었습니다.");

                bool dataStarted = false;
                int headerCount = 0;
                int processedLines = 0;
                int successfulLines = 0;

                foreach (string line in lines)
                {
                    try
                    {
                        if (line.StartsWith("#"))
                        {
                            headerCount++;
                            if (line.Contains("START7777"))
                            {
                                GMLogHelper.WriteLog("START7777 헤더를 찾았습니다.");
                                dataStarted = true;
                            }
                            continue;
                        }

                        if (!dataStarted)
                        {
                            continue;
                        }

                        if (line.Contains("7777END"))
                        {
                            GMLogHelper.WriteLog("7777END 마커를 찾았습니다.");
                            break;
                        }

                        processedLines++;
                        string[] splitedData = line.Split(new char[] { ',' });

                        if (splitedData.Length < 55)
                        {
                            GMLogHelper.WriteLog($"경고: 데이터 형식이 잘못되었습니다 - {line}");
                            continue;
                        }

                        rcvKMAASOSData addData = new rcvKMAASOSData()
                        {
                            TM = splitedData[0].Trim(),
                            STN = BizCommon.IntConvert(splitedData[1].Trim()),
                            WS_AVG = BizCommon.DoubleConvert(splitedData[2].Trim()),
                            WR_DAY = BizCommon.DoubleConvert(splitedData[3].Trim()),
                            WD_MAX = BizCommon.DoubleConvert(splitedData[4].Trim()),
                            WS_MAX = BizCommon.DoubleConvert(splitedData[5].Trim()),
                            WS_MAX_TM = BizCommon.DoubleConvert(splitedData[6].Trim()),
                            WD_INS = BizCommon.DoubleConvert(splitedData[7].Trim()),
                            WS_INS = BizCommon.DoubleConvert(splitedData[8].Trim()),
                            WS_INS_TM = BizCommon.DoubleConvert(splitedData[9].Trim()),
                            TA_AVG = BizCommon.DoubleConvert(splitedData[10].Trim()),
                            TA_MAX = BizCommon.DoubleConvert(splitedData[11].Trim()),
                            TA_MAX_TM = BizCommon.DoubleConvert(splitedData[12].Trim()),
                            TA_MIN = BizCommon.DoubleConvert(splitedData[13].Trim()),
                            TA_MIN_TM = BizCommon.DoubleConvert(splitedData[14].Trim()),
                            TD_AVG = BizCommon.DoubleConvert(splitedData[15].Trim()),
                            TS_AVG = BizCommon.DoubleConvert(splitedData[16].Trim()),
                            TG_MIN = BizCommon.DoubleConvert(splitedData[17].Trim()),
                            HM_AVG = BizCommon.DoubleConvert(splitedData[18].Trim()),
                            HM_MIN = BizCommon.DoubleConvert(splitedData[19].Trim()),
                            HM_MIN_TM = BizCommon.DoubleConvert(splitedData[20].Trim()),
                            PV_AVG = BizCommon.DoubleConvert(splitedData[21].Trim()),
                            EV_S = BizCommon.DoubleConvert(splitedData[22].Trim()),
                            EV_L = BizCommon.DoubleConvert(splitedData[23].Trim()),
                            FG_DUR = BizCommon.DoubleConvert(splitedData[24].Trim()),
                            PA_AVG = BizCommon.DoubleConvert(splitedData[25].Trim()),
                            PS_AVG = BizCommon.DoubleConvert(splitedData[26].Trim()),
                            PS_MAX = BizCommon.DoubleConvert(splitedData[27].Trim()),
                            PS_MAX_TM = BizCommon.DoubleConvert(splitedData[28].Trim()),
                            PS_MIN = BizCommon.DoubleConvert(splitedData[29].Trim()),
                            PS_MIN_TM = BizCommon.DoubleConvert(splitedData[30].Trim()),
                            CA_TOT = BizCommon.DoubleConvert(splitedData[31].Trim()),
                            SS_DAY = BizCommon.DoubleConvert(splitedData[32].Trim()),
                            SS_DUR = BizCommon.DoubleConvert(splitedData[33].Trim()),
                            SS_CMB = BizCommon.DoubleConvert(splitedData[34].Trim()),
                            SI_DAY = BizCommon.DoubleConvert(splitedData[35].Trim()),
                            SI_60M_MAX = BizCommon.DoubleConvert(splitedData[36].Trim()),
                            SI_60M_MAX_TM = BizCommon.DoubleConvert(splitedData[37].Trim()),
                            RN_DAY = BizCommon.DoubleConvert(splitedData[38].Trim()),
                            RN_D99 = BizCommon.DoubleConvert(splitedData[39].Trim()),
                            RN_DUR = BizCommon.DoubleConvert(splitedData[40].Trim()),
                            RN_60M_MAX = BizCommon.DoubleConvert(splitedData[41].Trim()),
                            RN_60M_MAX_TM = BizCommon.DoubleConvert(splitedData[42].Trim()),
                            RN_10M_MAX = BizCommon.DoubleConvert(splitedData[43].Trim()),
                            RN_10M_MAX_TM = BizCommon.DoubleConvert(splitedData[44].Trim()),
                            RN_POW_MAX = BizCommon.DoubleConvert(splitedData[45].Trim()),
                            RN_POW_MAX_TM = BizCommon.DoubleConvert(splitedData[46].Trim()),
                            SD_NEW = BizCommon.DoubleConvert(splitedData[47].Trim()),
                            SD_NEW_TM = BizCommon.DoubleConvert(splitedData[48].Trim()),
                            SD_MAX = BizCommon.DoubleConvert(splitedData[49].Trim()),
                            SD_MAX_TM = BizCommon.DoubleConvert(splitedData[50].Trim()),
                            TE_05 = BizCommon.DoubleConvert(splitedData[51].Trim()),
                            TE_10 = BizCommon.DoubleConvert(splitedData[52].Trim()),
                            TE_15 = BizCommon.DoubleConvert(splitedData[53].Trim()),
                            TE_30 = BizCommon.DoubleConvert(splitedData[54].Trim()),
                            TE_50 = BizCommon.DoubleConvert(splitedData[55].Trim())
                        };

                        listKMAASOSData.Add(addData);
                        successfulLines++;
                    }
                    catch (Exception lineEx)
                    {
                        GMLogHelper.WriteLog($"줄 처리 중 오류 발생: {lineEx.Message}");
                        continue;
                    }
                }

                GMLogHelper.WriteLog($"파일 처리 완료 - 헤더 {headerCount}개, 처리된 줄 {processedLines}개, 성공한 줄 {successfulLines}개");
                return listKMAASOSData;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"파일 처리 중 오류 발생: {ex.Message}");
                GMLogHelper.WriteLog($"StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
