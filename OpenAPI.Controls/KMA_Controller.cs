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

        public static List<rcvKMAASOSData> FiletoList_KMAASOS(string filePath)
        {
            List<rcvKMAASOSData> listKMAASOS = new List<rcvKMAASOSData>();

            using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
            {
                string strline = string.Empty;

                while (sr.Peek() > 0)
                {
                    strline = sr.ReadLine();

                    if (strline.Contains("#") == true)
                    {
                        continue;
                    }
                    else
                    {
                        //////         string[] vals = strline.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        ///string[] vals = strline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] vals;
                        if (strline.Contains(","))
                        {
                            // 쉼표로 구분된 데이터 처리
                            vals = strline.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            // 공백으로 구분된 데이터 처리
                            vals = strline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (vals.Length == 56)
                        {
                            rcvKMAASOSData addData = new rcvKMAASOSData();

                            addData.TM = vals[0].Trim();
                            addData.STN = int.Parse(vals[1].Trim());
                            addData.WS_AVG = double.Parse(vals[2].Trim());
                            addData.WR_DAY = double.Parse(vals[3].Trim());
                            addData.WD_MAX = double.Parse(vals[4].Trim());
                            addData.WS_MAX = double.Parse(vals[5].Trim());
                            addData.WS_MAX_TM = double.Parse(vals[6].Trim());
                            addData.WD_INS = double.Parse(vals[7].Trim());
                            addData.WS_INS = double.Parse(vals[8].Trim());
                            addData.WS_INS_TM = double.Parse(vals[9].Trim());
                            addData.TA_AVG = double.Parse(vals[10].Trim());
                            addData.TA_MAX = double.Parse(vals[11].Trim());
                            addData.TA_MAX_TM = double.Parse(vals[12].Trim());
                            addData.TA_MIN = double.Parse(vals[13].Trim());
                            addData.TA_MIN_TM = double.Parse(vals[14].Trim());
                            addData.TD_AVG = double.Parse(vals[15].Trim());
                            addData.TS_AVG = double.Parse(vals[16].Trim());
                            addData.TG_MIN = double.Parse(vals[17].Trim());
                            addData.HM_AVG = double.Parse(vals[18].Trim());
                            addData.HM_MIN = double.Parse(vals[19].Trim());
                            addData.HM_MIN_TM = double.Parse(vals[20].Trim());
                            addData.PV_AVG = double.Parse(vals[21].Trim());
                            addData.EV_S = double.Parse(vals[22].Trim());
                            addData.EV_L = double.Parse(vals[23].Trim());
                            addData.FG_DUR = double.Parse(vals[24].Trim());
                            addData.PA_AVG = double.Parse(vals[25].Trim());
                            addData.PS_AVG = double.Parse(vals[26].Trim());
                            addData.PS_MAX = double.Parse(vals[27].Trim());
                            addData.PS_MAX_TM = double.Parse(vals[28].Trim());
                            addData.PS_MIN = double.Parse(vals[29].Trim());
                            addData.PS_MIN_TM = double.Parse(vals[30].Trim());
                            addData.CA_TOT = double.Parse(vals[31].Trim());
                            addData.SS_DAY = double.Parse(vals[32].Trim());
                            addData.SS_DUR = double.Parse(vals[33].Trim());
                            addData.SS_CMB = double.Parse(vals[34].Trim());
                            addData.SI_DAY = double.Parse(vals[35].Trim());
                            addData.SI_60M_MAX = double.Parse(vals[36].Trim());
                            addData.SI_60M_MAX_TM = double.Parse(vals[37].Trim());
                            addData.RN_DAY = double.Parse(vals[38].Trim());
                            addData.RN_D99 = double.Parse(vals[39].Trim());
                            addData.RN_DUR = double.Parse(vals[40].Trim());
                            addData.RN_60M_MAX = double.Parse(vals[41].Trim());
                            addData.RN_60M_MAX_TM = double.Parse(vals[42].Trim());
                            addData.RN_10M_MAX = double.Parse(vals[43].Trim());
                            addData.RN_10M_MAX_TM = double.Parse(vals[44].Trim());
                            addData.RN_POW_MAX = double.Parse(vals[45].Trim());
                            addData.RN_POW_MAX_TM = double.Parse(vals[46].Trim());
                            addData.SD_NEW = double.Parse(vals[47].Trim());
                            addData.SD_NEW_TM = double.Parse(vals[48].Trim());
                            addData.SD_MAX = double.Parse(vals[49].Trim());
                            addData.SD_MAX_TM = double.Parse(vals[50].Trim());
                            addData.TE_05 = double.Parse(vals[51].Trim());
                            addData.TE_10 = double.Parse(vals[52].Trim());
                            addData.TE_15 = double.Parse(vals[53].Trim());
                            addData.TE_30 = double.Parse(vals[54].Trim());
                            addData.TE_50 = double.Parse(vals[55].Trim());

                            listKMAASOS.Add(addData);
                        }
                    }
                }
            }

            return listKMAASOS;
        }
    }
}
