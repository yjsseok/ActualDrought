using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace SoilMoisture_Server
{
    public class ServerEventArgs : EventArgs
    {
        public string text { get; }    // 파라메터로 넘겨 줄 데이타

        public ServerEventArgs(string text)   // 생성자에서 변경된 Text 정보를 넘겨받는다.
        {
            this.text = text;
        }
    }

    public delegate void ServerEventHandler(object sender, ServerEventArgs e);

    public class ClientHandle
    {
        private Socket socket;
        private NetworkStream networkstream = null;
        private StreamReader streamReader = null;
        private StreamWriter streamWriter = null;
        public event ServerEventHandler OnClientReceive = null;

        private static readonly ILog log = LogManager.GetLogger(typeof(ClientHandle));

        public ClientHandle(Socket client)
        {
            //log.Info("ClientHandle : new...");

            try
            {
                this.socket = client;
                networkstream = new NetworkStream(client);

                streamReader = new StreamReader(networkstream, Encoding.GetEncoding("utf-8"));
                streamWriter = new StreamWriter(networkstream, Encoding.GetEncoding("utf-8")) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
        }

        private void ShowLog(string msg)
        {
            OnClientReceive(this, new ServerEventArgs(msg));
            log.Info(msg);
        }

        public void Run()
        {
            string receiveData;        // 데이타 수신이 null 이 될수 있으므로 null 참조 가능으로 선언한다. 

            try
            {
                ShowLog("ClientHandle : Run()");

                while (true)
                {
                    receiveData = streamReader.ReadLine();     // NetworkStream 으로 들어오는 데이타의 끝은 CR, LF 가 필요하다. 

                    ShowLog(receiveData);

                    // CR LF 가 없으면 버퍼가 overflow 될때 까지 함수 내부에서 빠져 나오지 못한다. 
                    if (receiveData is null) break;             // Client 연결이 끊어지면 null 이 리턴된다. 

                    //WriteClientData(receiveData);  // Echo 기능 입니다. 다은 특별한 동작이 필요하면 이코드를 삭제하고 구현하시면 됩니다. 
                    InsertReceiveData(receiveData);
                }

                DisConnect();       // Client 연결과 관련된 부분을 모두 닫아 버린다. 
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                DisConnect();
            }
        }

        public void DisConnect()
        {
            try
            {
                ShowLog("ClientHandle : Disconnect()");

                streamReader?.Close();
                streamWriter?.Close();
                networkstream?.Close();

                socket.Close();
            }
            catch (Exception ex)
            {
                ShowLog(ex.Message);
                throw ex;
            }
        }
        //--------------------------------------------------------------------------------------------------------
        public void WriteClientData(string Message)
        {
            try
            {
                streamWriter?.WriteLine(Message);
            }
            catch (Exception ex)
            {
                ShowLog(ex.Message);
                throw ex;
            }
        }

        public void InsertReceiveData(string data)
        {
            DATA_TYPE dataType = DATA_TYPE.NONE;    // 자료타입

            List<ReceiveData> receiveDatas = null;

            try
            {
                ShowLog("InsertReceiveData() START");

                if (string.IsNullOrEmpty(data))
                {
                    ShowLog("Data NULL");
                    return;
                }

                try
                {
                    receiveDatas = JsonConvert.DeserializeObject<List<ReceiveData>>(data);
                }
                catch (Exception ex)
                {
                    ShowLog("Error JSON TYPE!!");
                    throw ex;
                }


                foreach (var receiveData in receiveDatas)
                {
                    string measureDT = receiveData.measureDT;
                    string deviceid = receiveData.deviceid;
                    //int value = receiveData.value;

                    // 장비 타입 (수위, 유속계)
                    if (deviceid.StartsWith("RF"))
                    {
                        dataType = DATA_TYPE.SPEED;
                    }
                    else if (deviceid.StartsWith("PL"))
                    {
                        dataType = DATA_TYPE.WL;
                    }
                    else
                    {
                        ShowLog("DataType : NONE");
                        return;
                    }

                    // check
                    if (string.IsNullOrEmpty(measureDT))
                    {
                        ShowLog("measureDT is NULL");
                        return;
                    }

                    if (string.IsNullOrEmpty(deviceid))
                    {
                        ShowLog("deviceid is NULL");
                        return;
                    }

                    //string errMsg = new DbLib().Add(dataType, measureDT, deviceid, value);

                    //if (string.IsNullOrEmpty(errMsg))
                    //{
                    //    ShowLog(string.Format("INSERT : {0},{1},{2},{3}", dataType, measureDT, deviceid, value));
                    //}
                    //else
                    //{
                    //    ShowLog(errMsg);
                    //}
                }

                ShowLog("InsertReceiveData() END");
            }
            catch (Exception ex)
            {
                ShowLog(ex.Message);
                throw ex;
            }
        }

        public void InsertReceiveData(string obsdh, string obscd, double data)
        {
            DATA_TYPE dataType = DATA_TYPE.NONE;

            try
            {
                if (obscd.Contains("PLB"))
                {
                    dataType = DATA_TYPE.WL;
                }

                if (dataType != DATA_TYPE.NONE)
                {
                    //string retVal = new DbLib().Add(dataType, obsdh, obscd, data);

                    //if (retVal.Length > 0)
                    //{
                    //    MessageBox.Show(retVal);
                    //}
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
        }
    }

   
}
