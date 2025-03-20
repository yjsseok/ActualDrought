using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFRI.FrameWork;

namespace SoilMoisture_Server
{
    public partial class frmMain : Form
    {
        #region [Delegate]
        delegate void WriteToStatusCallback(string message);    //스레드교착
        #endregion

        private MultiClientServer server = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Log
            GMLogManager.ConfigureLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"));
            this.WriteToStatus("Initialize...");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = 7000;

                server = new MultiClientServer(port);
                server.OnClientConnected += Server_OnClientConnected;
                this.WriteToStatus("서버 시작....");
                server.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.WriteToStatus("서버 중지 완료");

            server.Stop();
        }

        private void Server_OnClientConnected(object sender, ServerEventArgs e)
        {
            this.WriteToStatus(e.text);
        }

        #region [Message 함수]
        private void WriteToStatus(string message)
        {
            Application.DoEvents();

            if (this.listStatus.InvokeRequired)
            {
                WriteToStatusCallback d = new WriteToStatusCallback(WriteToStatus);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                if (listStatus.Items.Count > 200)
                {
                    listStatus.Items.Remove(listStatus.Items.Count);
                }
                listStatus.Items.Insert(0, DateTime.Now + " - " + message);

                GMLogHelper.WriteLog(message);
            }

            Application.DoEvents();
        }
        #endregion

        
    }
}
