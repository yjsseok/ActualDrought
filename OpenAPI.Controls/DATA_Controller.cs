using OpenAPI.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UFRI.FrameWork;
using System;

namespace OpenAPI.Controls
{
    public class AgriDamController
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiUrl = "http://apis.data.go.kr/B552149/reserviorWaterLevel/reservoirlevel/";

        public async Task<List<ReservoirLevelData>> GetReservoirDataAsync(string damCode, DateTime startDate, DateTime endDate)
        {
            var result = new List<ReservoirLevelData>();
            string formattedStartDate = startDate.ToString("yyyyMMdd");
            string formattedEndDate = endDate.ToString("yyyyMMdd");
            string requestUrl = $"{_apiUrl}?serviceKey={Config.DATA_ApiKey2}&pageNo=1&numOfRows=1000&fac_code={damCode}&date_s={formattedStartDate}&date_e={formattedEndDate}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                string xmlResponse = Encoding.UTF8.GetString(byteArray);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlResponse);

                XmlNodeList itemNodes = xmlDoc.SelectNodes("//item");

                if (itemNodes != null)
                {
                    foreach (XmlNode item in itemNodes)
                    {
                        result.Add(new ReservoirLevelData
                        {
                            check_date = GetNodeValue(item, "check_date"),
                            county = GetNodeValue(item, "county"),
                            fac_code = GetNodeValue(item, "fac_code"),
                            fac_name = GetNodeValue(item, "fac_name"),
                            rate = GetNodeValue(item, "rate"),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"API 호출 오류 (저수지 코드: {damCode}): {ex.Message}");
                return null; // 오류 발생 시 null 반환
            }
            return result;
        }

        private string GetNodeValue(XmlNode parentNode, string nodeName)
        {
            XmlNode node = parentNode.SelectSingleNode(nodeName);
            return node != null ? node.InnerText : string.Empty;
        }
    }
}