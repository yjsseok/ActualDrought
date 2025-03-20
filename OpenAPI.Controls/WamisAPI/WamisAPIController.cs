using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFRI.FrameWork;

namespace OpenAPI.Controls
{
    public class WamisAPIController
    {
        public DataTable GetDamSiteData(string dataType)
        {
            WamisAPIService apiService = new WamisAPIService();
            DataTable rtnTable = apiService.getList_wkd(dataType, "");

            return rtnTable;
        }

        public DataTable GetObsData(string dataType)
        {
            WamisAPIService apiService = new WamisAPIService();
            ParamObj paramObj = new ParamObj(dataType); // 파라미터 객체

            return paramObj.obscdDT;
        }

        public DataTable GetObsData_DataTable(string dataType)
        {
            WamisAPIService apiService = new WamisAPIService();
            ParamObj paramObj = new ParamObj(dataType); // 파라미터 객체
            DataTable rtnTable = new DataTable();         // 저장할 테이블

            try
            {
                foreach (string colName in paramObj.colArr)
                {
                    rtnTable.Columns.Add(colName);
                }

                foreach (DataRow dr in paramObj.obscdDT.Rows)
                {
                    string obscd = dr["obscd"].ToString().Trim(); // 안성천 관측소 값 "11111   " 으로 나옴

                    //SplashScreenManager.Default.SetWaitFormDescription("관측소 코드 : " + obscd);

                    DataTable tempDT = apiService.getList(paramObj.apiAddr, "&obscd=" + obscd);
                    if (tempDT != null && obscd.Length == 8)
                    {
                        DataRow tempDR = rtnTable.NewRow();
                        tempDR.ItemArray = tempDT.DefaultView.ToTable(false, paramObj.colArr).Rows[0].ItemArray;
                        rtnTable.Rows.Add(tempDR);
                    }
                    else if (tempDT != null && obscd.Length == 7)//수위관측소
                    {
                        DataRow tempDR = rtnTable.NewRow();
                        tempDR.ItemArray = tempDT.DefaultView.ToTable(false, paramObj.colArr).Rows[0].ItemArray;
                        rtnTable.Rows.Add(tempDR);
                    }
                }

                return rtnTable;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                throw;
            }
        }

        public DataTable GetWLData_DataTable(string dataType, DataTable dtWLStation, DateTime sDate, DateTime eDate)
        {
            WamisAPIService apiService = new WamisAPIService();
            ParamObj paramObj = new ParamObj(dataType); // 파라미터 객체
            DataTable rtnTable = new DataTable();         // 저장할 테이블

            try
            {
                //헤더생성
                rtnTable.Columns.Add("ymd");

                foreach (DataRow dr in dtWLStation.Rows)
                {
                    rtnTable.Columns.Add(dr["wlobscd"].ToString().Trim());
                }

                //시간생성
                for (DateTime dt = sDate; dt <= eDate; dt = dt.AddDays(1))
                {
                    DataRow tempDR = rtnTable.NewRow();
                    tempDR["ymd"] = dt.ToString("yyyyMMdd");
                    rtnTable.Rows.Add(tempDR);
                }

                foreach (DataRow dr in dtWLStation.Rows)
                {
                    string obscd = dr["wlobscd"].ToString().Trim();
                    SplashScreenManager.Default.SetWaitFormDescription("관측소 코드 : " + obscd);

                    DataTable stationData = new DataTable();

                    stationData = apiService.getList(paramObj.apiAddr, string.Format("&obscd={0}&startdt={1}&enddt={2}", obscd, sDate.ToString("yyyyMMdd"), eDate.ToString("yyyyMMdd")));

                    if (stationData != null)
                    {
                        foreach (DataRow tempDR in stationData.Rows)
                        {
                            rtnTable.Select("ymd=" + tempDR["ymd"])[0][obscd] = tempDR["wl"];
                        }
                    }
                }


                return rtnTable;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                throw;
            }
        }

        public DataTable GetFlowData_DataTable(string dataType, DataTable dtFlowStation, DateTime sDate, DateTime eDate)
        {
            WamisAPIService apiService = new WamisAPIService();
            ParamObj paramObj = new ParamObj(dataType); // 파라미터 객체
            DataTable rtnTable = new DataTable();         // 저장할 테이블

            try
            {
                //헤더생성
                rtnTable.Columns.Add("ymd");

                foreach (DataRow dr in dtFlowStation.Rows)
                {
                    rtnTable.Columns.Add(dr["wlobscd"].ToString().Trim());
                }

                DateTime stDate = new DateTime(sDate.Year, 1, 1);
                DateTime edDate = new DateTime(eDate.Year, 12, 31);

                //시간생성
                for (DateTime dt = stDate; dt <= edDate; dt = dt.AddDays(1))
                {
                    DataRow tempDR = rtnTable.NewRow();
                    tempDR["ymd"] = dt.ToString("yyyyMMdd");
                    rtnTable.Rows.Add(tempDR);
                }

                for (int i = sDate.Year; i <= eDate.Year; i++)
                {
                    foreach (DataRow dr in dtFlowStation.Rows)
                    {
                        string obscd = dr["wlobscd"].ToString().Trim();
                        SplashScreenManager.Default.SetWaitFormDescription("관측소 코드 : " + obscd);

                        DataTable stationData = new DataTable();

                        stationData = apiService.getList(paramObj.apiAddr, string.Format("&obscd={0}&year={1}", obscd, i));

                        if (stationData != null)
                        {
                            foreach (DataRow tempDR in stationData.Rows)
                            {
                                rtnTable.Select("ymd=" + tempDR["ymd"])[0][obscd] = tempDR["fw"];
                            }
                        }
                    }
                }

                DataTable resultTable = new DataTable();

                //resultTable = rtnTable.Select(string.Format("ymd < {0} AND ymd > {1}", sDate.ToString("yyyyMMdd"), eDate.ToString("yyyyMMdd"))).CopyToDataTable();

                DataTable tb1 = rtnTable.Select(string.Format("ymd >= {0}", sDate.ToString("yyyyMMdd"))).CopyToDataTable();
                DataTable tb2 = tb1.Select(string.Format("ymd <= {0}", eDate.ToString("yyyyMMdd"))).CopyToDataTable();

                return tb2;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                throw;
            }

        }

        public DataTable Get_mn_dammain_All_DataTable(string serviceName)
        {
            WamisAPIService apiService = new WamisAPIService();
            WamisParamObj paramObj = new WamisParamObj(serviceName);
            DataTable rtnTable = new DataTable();

            try
            {
                rtnTable = apiService.getList("mn_dammain", "");
                //foreach (string colName in paramObj.colArr)
                //{
                //    rtnTable.Columns.Add(colName);
                //}

                //foreach (DataRow dr in paramObj.dtDamCD.Rows)
                //{
                //    string damCD = dr["damcd"].ToString().Trim();

                //    SplashScreenManager.Default.SetWaitFormDescription("댐 코드 : " + damCD);

                //    DataTable tempDT = apiService.getList(paramObj.apiAddr, "&damcd=" + damCD);
                //    if (tempDT != null)
                //    {
                //        DataRow tempDR = rtnTable.NewRow();
                //        tempDR.ItemArray = tempDT.DefaultView.ToTable(false, paramObj.colArr).Rows[0].ItemArray;
                //        rtnTable.Rows.Add(tempDR);
                //    }
                //}

                return rtnTable;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
