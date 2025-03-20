using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoilMoisture_Server
{
    public class MultiClientServer
    {
        private TcpListener server = null;
        public event ServerEventHandler OnClientConnected = null;
        public event ServerEventHandler OnServerClosed = null;

        public string IpAddress { set; get; } = null;

        public int Port { set; get; } = 5000;

        public MultiClientServer()
        {
            try
            {
                this.IpAddress = GetLocalIP();
                Enable();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public MultiClientServer(int port)
        {
            try
            {
                this.IpAddress = GetLocalIP();
                this.Port = port;

                Enable();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetLocalIP()
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                string ip = string.Empty;

                foreach (var address in host.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ip = address.ToString();
                        break;
                    }
                }

                return ip;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Enable()
        {
            try
            {
                if (this.IpAddress == null) { return false; }
                else
                {
                    IPEndPoint local = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
                    server = new TcpListener(local);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Start()
        {
            try
            {
                if (server != null)
                {
                    server.Start();              // Server 동작             

                    Thread connect = new Thread(WaitClientConnect);  // Client 접속 대기, Data 수신
                    connect.IsBackground = true;
                    connect.Start();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Stop()
        {
            try
            {
                server?.Stop();     // server 동작을 종료한다. 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void WaitClientConnect()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        Socket client = server?.AcceptSocket();   // Client  접속을 기다린다. 

                        if (client != null)
                        {
                            var ipClient = ((IPEndPoint)client.RemoteEndPoint).ToString();  // 접속된 Client의 IP 확인 

                            if (OnClientConnected != null) OnClientConnected(this, new ServerEventArgs(ipClient));   // Client 연결에 대한 이벤트 함수 실행 

                            ClientHandle clientHandle = new ClientHandle(client);
                            clientHandle.OnClientReceive += ClientHandle_OnClientReceive;
                            Thread connectedClient = new Thread(new ThreadStart(clientHandle.Run));  // 접속된 Client 의 객체를 생성 Thread 를 사용하여 동작 수행
                            connectedClient.IsBackground = true;
                            connectedClient.Start();
                        }
                    }
                    catch (Exception ex)         // server 기능을 stop 하게되면 예외가 발생한다. 
                    {
                        if (OnServerClosed != null)
                        {
                            OnServerClosed(this, new ServerEventArgs(ex.ToString()));  // server 기능 멈충에 대한 이벤트 발생 
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ClientHandle_OnClientReceive(object sender, ServerEventArgs e)
        {
            try
            {
                OnClientConnected(this, new ServerEventArgs(e.text));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
