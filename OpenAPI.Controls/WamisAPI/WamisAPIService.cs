using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Controls
{
    public class WamisAPIService
    {
        private static string apiKey = "f2525801002414a533ba8a44798a3720d4eff13b96";

        public DataTable getList(string apiAddr, string damcd, string sDate, string eDate)
        {
            string uriStr = "http://www.wamis.go.kr:8080/wamis/openapi/wkd/" + apiAddr + "?output=json";
            uriStr += "&damcd=" + damcd;
            uriStr += "&startdt=" + sDate;
            uriStr += "&enddt=" + eDate;
            uriStr += "&key=" + apiKey;
            Uri uri = new Uri(uriStr);
            string responseJson = ExecuteGetResponse(uri);

            string count = JsonConvert.DeserializeAnonymousType(responseJson, new { count = default(string) }).count;
            DataTable dt = null;

            if (count != "0")
            {
                dt = JsonConvert.DeserializeAnonymousType(responseJson, new { list = default(DataTable) }).list;
            }

            return dt;
        }

        public DataTable getList(string apiAddr, string _param)
        {
            string uriStr = "http://www.wamis.go.kr:8080/wamis/openapi/wkw/" + apiAddr + "?output=json" + _param;
            uriStr += "&key=" + apiKey;
            Uri uri = new Uri(uriStr); //왜 uri를 사용하는지 모르겠음 ->값이 같은데?
            string responseJson = ExecuteGetResponse(uri);

            string count = JsonConvert.DeserializeAnonymousType(responseJson, new { count = default(string) }).count;
            DataTable dt = null;

            if (count != "0")
            {
                dt = JsonConvert.DeserializeAnonymousType(responseJson, new { list = default(DataTable) }).list;
            }

            return dt;
        }

        public DataTable getList_wkd(string apiAddr, string _param)
        {
            string uriStr = "http://www.wamis.go.kr:8080/wamis/openapi/wkd/" + apiAddr + "?output=json" + _param;
            uriStr += "&key=" + apiKey;
            Uri uri = new Uri(uriStr); //왜 uri를 사용하는지 모르겠음 ->값이 같은데?
            string responseJson = ExecuteGetResponse(uri);

            string count = JsonConvert.DeserializeAnonymousType(responseJson, new { count = default(string) }).count;
            DataTable dt = null;

            if (count != "0")
            {
                dt = JsonConvert.DeserializeAnonymousType(responseJson, new { list = default(DataTable) }).list;
            }

            return dt;
        }

        public DataTable getList_wkw(string apiAddr, string _param)
        {
            string uriStr = "http://www.wamis.go.kr:8080/wamis/openapi/wkw/" + apiAddr + "?output=json" + _param;
            uriStr += "&key=" + apiKey;
            Uri uri = new Uri(uriStr); //왜 uri를 사용하는지 모르겠음 ->값이 같은데?
            string responseJson = ExecuteGetResponse(uri);

            string count = JsonConvert.DeserializeAnonymousType(responseJson, new { count = default(string) }).count;
            DataTable dt = null;

            if (count != "0")
            {
                dt = JsonConvert.DeserializeAnonymousType(responseJson, new { list = default(DataTable) }).list;
            }

            return dt;
        }

        private static string ExecuteGetResponse(Uri baseUrl)
       {
            string sResponse = "";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseUrl);
                req.Credentials = CredentialCache.DefaultCredentials;
                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    StreamReader sr = new StreamReader(res.GetResponseStream());
                    sResponse = sr.ReadToEnd();
                }
            }
            catch (ArgumentException)
            {
                sResponse = "{\"result\": {\"type\": \"ERROR\"}}";
            }
            catch (System.Net.WebException)
            {
                sResponse = "{\"result\": {\"type\": \"ERROR\"}}";
            }
            catch (System.Net.Sockets.SocketException)
            {
                sResponse = "{\"result\": {\"type\": \"ERROR\"}}";
            }
            return sResponse;
        }
    }
}
