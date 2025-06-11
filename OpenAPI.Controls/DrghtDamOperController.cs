using System.Net.Http;
using System.Threading.Tasks;
using U8Xml;
using System.Collections.Generic;
using System.Linq;
using OpenAPI.Model;
using System;
using UFRI.FrameWork;
namespace OpenAPI.Controls
{
    public class DrghtDamOperController
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ServiceKey = "FpAShNYZTSjw5iNsUwVK867BWOExI9aW6YstOhSMmgEEquLAatpmvK9ZvuqaKJsKY%2BVAuuSlChy%2BP2xhEYDq6g%3D%3D";

        public async Task<List<DrghtDamOperData>> GetDamOperDataAsync(string damcd, string stDt, string edDt)
        {
            var allData = new List<DrghtDamOperData>();
            int pageNo = 1;
            int totalCount = 0;
            int numOfRows = 100;
            int totalPages = 0; // 외부 스코프에서 선언

            do
            {
                string url = $"http://apis.data.go.kr/B500001/drghtDamOper/operInfoList?ServiceKey={ServiceKey}&pageNo={pageNo}&numOfRows={numOfRows}&damCd={damcd}&stDt={stDt}&edDt={edDt}";
                string xmlContent = await _httpClient.GetStringAsync(url);
                var (dataList, currentTotalCount) = ParseXmlData(xmlContent);

                // 첫 페이지에서 전체 페이지 수 계산
                if (pageNo == 1)
                {
                    totalCount = currentTotalCount;
                    totalPages = (int)Math.Ceiling((double)totalCount / numOfRows);
                }

                allData.AddRange(dataList);
                pageNo++;
            } while (pageNo <= totalPages);

            return allData;
        }

        private (List<DrghtDamOperData> data, int totalCount) ParseXmlData(string xmlContent)
        {
            var dataList = new List<DrghtDamOperData>();
            int totalCount = 0;

            using (var xml = XmlParser.Parse(xmlContent))
            {
                var bodyNode = xml.Root.FindChild("body");
                if (!bodyNode.IsNull)
                {
                    // totalCount 추출
                    var totalCountNode = bodyNode.FindChild("totalCount");
                    totalCount = int.Parse(totalCountNode.InnerText.ToString());

                    // items 처리
                    var itemsNode = bodyNode.FindChild("items");
                    if (!itemsNode.IsNull)
                    {
                        foreach (var itemNode in itemsNode.Children.Where(x => x.Name == "item"))
                        {
                            var data = new DrghtDamOperData
                            {
                                damcd = itemNode.FindChild("damcd").InnerText.ToString(),
                                damnm = itemNode.FindChild("damnm").InnerText.ToString(),
                                iqty = ParseDouble(itemNode.FindChild("iqty")),
                                lwl = ParseDouble(itemNode.FindChild("lwl")),
                                obsymd = itemNode.FindChild("obsymd").InnerText.ToString(),
                                rsqty = ParseDouble(itemNode.FindChild("rsqty")),
                                rsrt = ParseDouble(itemNode.FindChild("rsrt"))
                            };
                            dataList.Add(data);
                        }
                    }
                }
            }
            return (dataList, totalCount);
        }

        private double ParseDouble(XmlNode node)
        {
            if (node.IsNull) return 0;
            return double.TryParse(node.InnerText.ToString(), out double result) ? result : 0;
        }
    }

}
