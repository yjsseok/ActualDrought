using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAPI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAPI.Controls
{
    public class WAMIS_Controller
    {
        public static async Task<List<DamHRData>> GetDamHrDataAsync(string damcd, DateTime search_stDate, DateTime search_edDate)
        {
            string url = string.Format("http://www.wamis.go.kr:8080/wamis/openapi/wkd/mn_hrdata?output=json&damcd={0}&startdt={1}&enddt={2}", damcd, search_stDate.ToString("yyyyMMdd"), search_edDate.ToString("yyyyMMdd"));

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // 200 OK 외의 상태코드에서 예외 발생

                    string json = await response.Content.ReadAsStringAsync();

                    // JSON 파싱 및 예외 처리
                    JObject jsonObject = JObject.Parse(json);
                    JArray dataArray = jsonObject["list"] as JArray; // as 연산자로 형변환

                    if (dataArray == null)
                    {
                        // "list" 키가 없거나 null인 경우 처리 (예: 로그 기록, 예외 발생)
                        Console.WriteLine("Error: 'list' key not found in JSON response.");
                        return null; // 또는 빈 리스트 반환: return new List<FlowData>();
                    }

                    List<DamHRData> damHrData = new List<DamHRData>();

                    foreach (JObject data in dataArray)
                    {
                        DamHRData addData = new DamHRData
                        {
                            damcd = damcd,
                            obsdh = data["obsdh"]?.ToString(), // null 처리 추가
                            rwl = data["rwl"]?.ToString(),
                            ospilwl = data["ospilwl"]?.ToString(),
                            rsqty = data["rsqty"]?.ToString(),
                            rsrt = data["rsrt"]?.ToString(),
                            iqty = data["iqty"]?.ToString(),
                            etqty = data["etqty"]?.ToString(),
                            tdqty = data["tdqty"]?.ToString(),
                            edqty = data["edqty"]?.ToString(),
                            spdqty = data["spdqty"]?.ToString(),
                            otltdqty = data["otltdqty"]?.ToString(),
                            itqty = data["itqty"]?.ToString(),
                            dambsarf = data["dambsarf"]?.ToString()
                        };

                        damHrData.Add(addData);
                    }

                    return damHrData;
                }
                catch (HttpRequestException ex)
                {
                    // HTTP 요청 예외 처리 (예: 네트워크 오류)
                    Console.WriteLine($"HTTP Request Error: {ex.Message}");
                    return null;
                }
                catch (JsonReaderException ex)
                {
                    // JSON 파싱 예외 처리
                    Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                    return null;
                }
                catch (SocketException ex) // SocketException 처리
                {
                    Console.WriteLine($"SocketException 발생: {ex.Message}");
                    int retryCount = 3;
                    int retryDelay = 1000;
                    for (int i = 0; i < retryCount; i++)
                    {
                        Console.WriteLine($"재시도 {i + 1}회 시도...");
                        await Task.Delay(retryDelay);
                        var result = await GetDamHrDataAsync(damcd, search_stDate, search_edDate);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    Console.WriteLine("최대 재시도 횟수 초과.");
                    return null;
                }
                catch (Exception ex)
                {
                    // 기타 예외 처리
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null;
                }
            }
        }

        public static List<DamHRData> GetDamHrData(string damcd, DateTime search_stDate, DateTime search_edDate)
        {
            string url = string.Format("http://www.wamis.go.kr:8080/wamis/openapi/wkd/mn_hrdata?output=json&damcd={0}&startdt={1}&enddt={2}", damcd, search_stDate.ToString("yyyyMMdd"), search_edDate.ToString("yyyyMMdd"));

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 동기 메서드 사용
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode(); // 200 OK 외의 상태코드에서 예외 발생

                    string json = response.Content.ReadAsStringAsync().Result;

                    // JSON 파싱 및 예외 처리
                    JObject jsonObject = JObject.Parse(json);
                    JArray dataArray = jsonObject["list"] as JArray; // as 연산자로 형변환

                    if (dataArray == null)
                    {
                        // "list" 키가 없거나 null인 경우 처리 (예: 로그 기록, 예외 발생)
                        Console.WriteLine("Error: 'list' key not found in JSON response.");
                        return null; // 또는 빈 리스트 반환: return new List<FlowData>();
                    }

                    List<DamHRData> damHrData = new List<DamHRData>();

                    int i = 0;

                    foreach (JObject data in dataArray)
                    {
                        if (i == 4518)
                        {
                            int kkk = 0;
                        }
                        DamHRData addData = new DamHRData();

                        addData.damcd = damcd;
                        addData.obsdh = data["obsdh"]?.ToString(); // null 처리 추가
                        addData.rwl = data["rwl"]?.ToString();
                        addData.ospilwl = data["ospilwl"]?.ToString();
                        addData.rsqty = data["rsqty"]?.ToString();
                        addData.rsrt = data["rsrt"]?.ToString();
                        addData.iqty = data["iqty"]?.ToString();
                        addData.etqty = data["etqty"]?.ToString();
                        addData.tdqty = data["tdqty"]?.ToString();
                        addData.edqty = data["edqty"]?.ToString();
                        addData.spdqty = data["spdqty"]?.ToString();
                        addData.otltdqty = data["otltdqty"]?.ToString();
                        addData.itqty = data["itqty"]?.ToString();
                        addData.dambsarf = data["dambsarf"]?.ToString();

                        damHrData.Add(addData);
                        Debug.Print(i.ToString());
                        i++;
                    }

                    return damHrData;
                }
                catch (HttpRequestException ex)
                {
                    // HTTP 요청 예외 처리 (예: 네트워크 오류)
                    Console.WriteLine($"HTTP Request Error: {ex.Message}");
                    return null;
                }
                catch (JsonReaderException ex)
                {
                    // JSON 파싱 예외 처리
                    Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                    return null;
                }
                catch (SocketException ex) // SocketException 처리
                {
                    Console.WriteLine($"SocketException 발생: {ex.Message}");
                    int retryCount = 3;
                    int retryDelay = 1000;
                    for (int i = 0; i < retryCount; i++)
                    {
                        Console.WriteLine($"재시도 {i + 1}회 시도...");
                        Thread.Sleep(retryDelay); // Task.Delay 대신 Thread.Sleep 사용
                        var result = GetDamHrData(damcd, search_stDate, search_edDate); // 재귀 호출
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    Console.WriteLine("최대 재시도 횟수 초과.");
                    return null;
                }
                catch (Exception ex)
                {
                    // 기타 예외 처리
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null;
                }
            }
        }

        public static async Task<List<FlowData>> GetFlowDataAsync(string obsCD, int year)
        {
            string url = string.Format("http://www.wamis.go.kr:8080/wamis/openapi/wkw/flw_dtdata?output=json&obscd={0}&year={1}", obsCD, year);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // 200 OK 외의 상태코드에서 예외 발생

                    string json = await response.Content.ReadAsStringAsync();

                    // JSON 파싱 및 예외 처리
                    JObject jsonObject = JObject.Parse(json);
                    JArray dataArray = jsonObject["list"] as JArray; // as 연산자로 형변환

                    if (dataArray == null)
                    {
                        // "list" 키가 없거나 null인 경우 처리 (예: 로그 기록, 예외 발생)
                        Console.WriteLine("Error: 'list' key not found in JSON response.");
                        return null; // 또는 빈 리스트 반환: return new List<FlowData>();
                    }


                    List<FlowData> flowDataList = new List<FlowData>();

                    foreach (JObject data in dataArray)
                    {
                        FlowData flowData = new FlowData
                        {
                            obscd = obsCD,
                            ymd = data["ymd"]?.ToString(), // null 처리 추가
                            flw = ParseFlowValue(data["fw"]) // 별도의 메서드로 파싱
                        };

                        flowDataList.Add(flowData);
                    }

                    return flowDataList;

                }
                catch (HttpRequestException ex)
                {
                    // HTTP 요청 예외 처리 (예: 네트워크 오류)
                    Console.WriteLine($"HTTP Request Error: {ex.Message}");
                    return null;
                }
                catch (JsonReaderException ex)
                {
                    // JSON 파싱 예외 처리
                    Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                    return null;
                }
                catch (SocketException ex) // SocketException 처리
                {
                    Console.WriteLine($"SocketException 발생: {ex.Message}");
                    int retryCount = 3;
                    int retryDelay = 1000;
                    for (int i = 0; i < retryCount; i++)
                    {
                        Console.WriteLine($"재시도 {i + 1}회 시도...");
                        await Task.Delay(retryDelay);
                        var result = await GetFlowDataAsync(obsCD, year);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    Console.WriteLine("최대 재시도 횟수 초과.");
                    return null;
                }
                catch (Exception ex)
                {
                    // 기타 예외 처리
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null;
                }
                //HttpResponseMessage response = await client.GetAsync(url);
                //if (response.IsSuccessStatusCode)
                //{
                //    string json = await response.Content.ReadAsStringAsync();

                //    // JSON 파싱
                //    JObject jsonObject = JObject.Parse(json);
                //    JArray dataArray = (JArray)jsonObject["list"];

                //    List<FlowData> flowDataList = new List<FlowData>();

                //    foreach (JObject data in dataArray)
                //    {
                //        FlowData flowData = new FlowData();

                //        flowData.obscd = obsCD;
                //        flowData.ymd = data["ymd"].ToString();
                //        flowData.flw = data["fw"].ToString() == "-" ? double.NaN : data["fw"].ToObject<double>();

                //        flowDataList.Add(flowData);
                //    }

                //    return flowDataList;
                //}
                //else
                //{
                //    return null;
                //}
            }
        }

        private static double ParseFlowValue(JToken fwToken)
        {
            if (fwToken == null) return double.NaN; // null 처리

            string fwValue = fwToken.ToString();

            if (fwValue == "-")
            {
                return double.NaN;
            }

            if (double.TryParse(fwValue, out double result))
            {
                return result;
            }

            return double.NaN; // 파싱 실패 시 NaN 반환
        }
    }
}
