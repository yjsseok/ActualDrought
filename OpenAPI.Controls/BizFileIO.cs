using CsvHelper;
using CsvHelper.Configuration;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UFRI.FrameWork;

namespace OpenAPI.Controls
{
    public class BizFileIO
    {
        public static List<T> ReadCSV<T>(string filePath) where T : new()
        {
            var lines = File.ReadAllLines(filePath, Encoding.Default);
            if (lines.Length == 0)
            {
                throw new Exception("CSV 파일이 비어 있습니다.");
            }

            var headers = lines[0].Split(',');
            var properties = typeof(T).GetProperties();

            List<T> result = new List<T>();

            foreach (var line in lines.Skip(1)) // 첫 줄(헤더)은 건너뛴다.
            {
                var values = line.Split(',');
                T obj = new T();

                for (int i = 0; i < headers.Length; i++)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(headers[i], StringComparison.OrdinalIgnoreCase));
                    if (prop != null && i < values.Length)
                    {
                        object convertedValue = Convert.ChangeType(values[i], prop.PropertyType);
                        prop.SetValue(obj, convertedValue);
                    }
                }
                result.Add(obj);
            }

            return result;
        }

        public static List<MatchingTable> ReadMatchingTable(string filePath)
        {
            try
            {
                List<MatchingTable> listMatching = new List<MatchingTable>();
                using (var reader = new StreamReader(filePath, Encoding.Default))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false // 헤더를 무시하고 데이터만 읽음
                }))
                {
                    listMatching = new List<MatchingTable>(csv.GetRecords<MatchingTable>());
                }
          

                return listMatching;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                return null;
            }
        }

        public static List<ModelKK> ReadModelKK(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<ModelKK> listResult = new List<ModelKK>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (vals.Length == 6)
                        {
                            ModelKK addData = new ModelKK();
                            addData.modelDate = DateTime.Parse(vals[0].Trim());
                            addData.sgCode = sgCD;
                            if (vals[1].Trim() != "")
                            {
                                addData.PCP = double.Parse(vals[1].Trim());
                            }

                            if (vals[2].Trim() != "")
                            {
                                addData.SPI1 = double.Parse(vals[2].Trim());
                            }

                            if (vals[3].Trim() != "")
                            {
                                addData.SM = double.Parse(vals[3].Trim());
                            }

                            if (vals[4].Trim() != "")
                            {
                                addData.SM_RDA = double.Parse(vals[4].Trim());
                            }

                            if (vals[5].Trim() != "")
                            {
                                addData.SSMI1 = double.Parse(vals[5].Trim());
                            }

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                return null;
            }
            
        }

        public static List<ModelSGAGRPADDY> ReadModelSGAGRPADDY(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<ModelSGAGRPADDY> listResult = new List<ModelSGAGRPADDY>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (vals.Length == 21)
                        {
                            ModelSGAGRPADDY addData = new ModelSGAGRPADDY();

                            string strDate = vals[2].Replace("\"", "").Trim();
                            addData.modelDate = DateTime.Parse(strDate);
                            addData.sgCode = sgCD;
                            addData.SRSI_A = double.Parse(vals[11].Trim());
                            addData.SRSI_A_drt = double.Parse(vals[19].Trim());

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<ModelSGHYD> ReadModelSGHYD(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<ModelSGHYD> listResult = new List<ModelSGHYD>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (vals.Length == 21)
                        {
                            ModelSGHYD addData = new ModelSGHYD();

                            string strDate = vals[2].Replace("\"", "").Trim();
                            addData.modelDate = DateTime.Parse(strDate);
                            addData.sgCode = sgCD;
                            addData.SRSI_H = double.Parse(vals[12].Trim());
                            addData.SRSI_H_drt = double.Parse(vals[20].Trim());

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<ModelSGRSRV> ReadModelSGRSRV(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<ModelSGRSRV> listResult = new List<ModelSGRSRV>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        int totalIdx = vals.Length - 1;

                        ModelSGRSRV addData = new ModelSGRSRV();
                        addData.modelDate = DateTime.Parse(vals[0].Trim());
                        addData.sgCode = sgCD;
                        addData.storageSum = double.Parse(vals[totalIdx].Trim());

                        listResult.Add(addData);
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<ModelSGFLSanrm> ReadModelSGFLSanrm(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<ModelSGFLSanrm> listResult = new List<ModelSGFLSanrm>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (vals.Length == 5)
                        {
                            ModelSGFLSanrm addData = new ModelSGFLSanrm();

                            addData.modelDate = DateTime.Parse((vals[0]).Trim());
                            addData.sgCode = vals[1].Trim();
                            addData.precipitation = double.Parse(vals[2].Trim());
                            addData.evaporation = double.Parse(vals[3].Trim());
                            addData.soilMoisture = double.Parse(vals[4].Trim());

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<ModelSGFLS> ReadModelSGFLS_Mon(string filePath)
        {
            try
            {
                List<ModelSGFLS> listResult = new List<ModelSGFLS>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (strline.ToUpper().Contains("NAN") == true)
                        {
                            int kkk = 0;
                        }

                        if (vals.Length == 3)
                        {
                            ModelSGFLS addData = new ModelSGFLS();
                            addData.modelDate = DateTime.Parse((vals[0]).Trim());
                            addData.sgCode = vals[1].Trim();
                            addData.FlashDroughtMonitor = int.Parse(vals[2].Trim());

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<ModelSGFLS> ReadModelSGFLS(string filePath)
        {
            try
            {
                List<ModelSGFLS> listResult = new List<ModelSGFLS>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (strline.ToUpper().Contains("NAN") == true)
                        {
                            int kkk = 0;
                        }

                        if (vals.Length == 4)
                        {
                            ModelSGFLS addData = new ModelSGFLS();
                            addData.modelDate = DateTime.Parse((vals[0]).Trim());
                            addData.sgCode = vals[1].Trim();

                            if (vals[2].Trim().ToUpper().Contains("NAN") == true)
                            {
                                addData.STVI = 0;
                            }
                            else
                            {
                                addData.STVI = double.Parse(vals[2].Trim());
                            }

                            if (vals[3].Trim().ToUpper().Contains("NAN") == true)
                            {
                                addData.EDDI_SPI = 0;
                            }
                            else
                            {
                                addData.EDDI_SPI = double.Parse(vals[3].Trim());
                            }                            

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static List<SoilMoisture> ReadSoilMoisture(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<SoilMoisture> listResult = new List<SoilMoisture>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (vals.Length == 7)
                        {
                            SoilMoisture addData = new SoilMoisture();

                            addData.measureDT = ConvertDateTime(vals[0].Trim());
                            addData.SiteCode = sgCD;
                            addData.wc10 = double.Parse(vals[1].Trim());
                            addData.wc20 = double.Parse(vals[2].Trim());
                            addData.wc30 = double.Parse(vals[3].Trim());
                            addData.wc40 = double.Parse(vals[4].Trim());
                            addData.wc50 = double.Parse(vals[5].Trim());
                            addData.bat = double.Parse(vals[6].Trim());

                            listResult.Add(addData);
                        }
                    }
                }
                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        private static DateTime ConvertDateTime(string dateString)
        {
            if (dateString.Length != 14)
            {
                throw new ArgumentException("The date string must be exactly 14 characters long.");
            }

            DateTime date;
            bool success = DateTime.TryParseExact(dateString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            return date;
        }

        public static List<ModelSGMT> ReadModelSGMT(string filePath)
        {
            try
            {
                string sgCD = Path.GetFileNameWithoutExtension(filePath);

                List<ModelSGMT> listResult = new List<ModelSGMT>();

                using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
                {
                    string strline = string.Empty;
                    strline = sr.ReadLine();

                    while (sr.Peek() > 0)
                    {
                        strline = sr.ReadLine();
                        string[] vals = strline.Split(new char[] { ',' });

                        if (vals.Length == 21)
                        {
                            ModelSGMT addData = new ModelSGMT();
                            string strDate = vals[2].Replace("\"", "").Trim();
                            addData.modelDate = DateTime.Parse(strDate);
                            addData.sgCode = sgCD;
                            addData.spi_7 = double.Parse(vals[5].Trim());
                            addData.spi_30 = double.Parse(vals[6].Trim());
                            addData.spi_90 = double.Parse(vals[7].Trim());
                            addData.spi_180 = double.Parse(vals[8].Trim());
                            addData.spi_270 = double.Parse(vals[9].Trim());
                            addData.spi_365 = double.Parse(vals[10].Trim());

                            addData.spi_7_dr = int.Parse(vals[13].Trim());
                            addData.spi_30_dr = int.Parse(vals[14].Trim());
                            addData.spi_90_dr = int.Parse(vals[15].Trim());
                            addData.spi_180_dr = int.Parse(vals[16].Trim());
                            addData.spi_270_dr = int.Parse(vals[17].Trim());
                            addData.spi_365_dr = int.Parse(vals[18].Trim());

                            listResult.Add(addData);
                        }
                    }
                }

                return listResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void WriteMIData(string dir, string sggcd, AreaRainfall sgg_AreaRainfall)
        {
            string writePath = Path.Combine(dir, string.Format("{0}.csv", sggcd));

            using (StreamWriter sw = new StreamWriter(writePath, false, Encoding.Default))
            {
                //Header 생성
                //년,월,일,JD,강우량,관측소코드...
                //yyyy,MM,dd,day(일수),면적강우,관측소별강우 (n개)
                string Header = string.Format("년,월,일,JD,강우량");

                foreach (PointRainfall pointRain in sgg_AreaRainfall.CollectionPointRainfall)
                {
                    Header += string.Format(",{0}_{1}", pointRain.stn, pointRain.ratio);
                }
                sw.WriteLine(Header);

                //Body 생성
                int i = 0;
                foreach (tsTimeSeries areaRainfall in sgg_AreaRainfall.listAreaRainfall)
                {
                    string Body = string.Format("{0},{1},{2},{3},{4}",areaRainfall.tmdt.Year, areaRainfall.tmdt.Month, areaRainfall.tmdt.Day, areaRainfall.tmdt.DayOfYear, areaRainfall.rainfall);

                    foreach (PointRainfall pointRain in sgg_AreaRainfall.CollectionPointRainfall)
                    {
                        Body += string.Format(",{0}", pointRain.listRainfall[i].rainfall);
                    }
                    
                    sw.WriteLine(Body);
                    i++;
                }                
            }
        }
    }
}
