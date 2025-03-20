using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI.Controls
{
    public class WamisParamObj
    {
        public DataTable dtDamCD { get; }        // 댐 목록
        public string[] colArr { get; }          // 데이터별 테이블 칼럼
        public string apiAddr { get; }           // 호출할 api 주소값
        public string saveFileName { get; }      // 저장할 엑셀 파일 명

        WamisAPIService apiService = new WamisAPIService();

        public WamisParamObj(string serviceName) 
        {
            if (serviceName == "mn_dammain") //댐검색
            {
                dtDamCD = apiService.getList_wkd("mn_dammain", "");
                colArr = new string[] { "damcd", "damnm", "bbsncd", "sbsncd", "bbsnnm", "mggvnm" };
                apiAddr = "mn_dammain";
                saveFileName = "mn_dammain";
            }
            else if (serviceName == "mn_hrdata") //시자료
            {
                dtDamCD = apiService.getList_wkd("mn_dammain", "");
                colArr = new string[] { "damcd", "obsdh", "rwl", "ospilwl", "rsqty", "rsrt", "iqty", "etqty", "tdqty", "edqty", "spdqty", "otltdqty", "itqty", "dambsarf" };
                apiAddr = "mn_hrdata";
                saveFileName = "mn_hrdata";
            }
            else if (serviceName == "mn_dtdata") //일자료
            {
                dtDamCD = apiService.getList("mn_dammain", "");
                colArr = new string[] { "obsdh", "rwl", "iqty", "tdqty", "edqty", "spdqty", "otltdqty", "itqty", "rf" };
                apiAddr = "mn_dtdata";
                saveFileName = "mn_dtdata";
            }
            else if (serviceName == "mn_mndata") //월자료
            {

            }
        }
    }
}
