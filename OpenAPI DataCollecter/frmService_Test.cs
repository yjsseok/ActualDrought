using DevExpress.XtraSplashScreen;
using OpenAPI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenAPI_DataCollecter
{
    public partial class frmService_Test : Form
    {
        public frmService_Test()
        {
            InitializeComponent();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnDamInfo_Req":
                    Request_DamInfo();
                    break;
            }
        }

        private void Request_DamInfo()
        {
            SplashScreenManager.ShowForm(typeof(frmWait));
            SplashScreenManager.Default.SetWaitFormCaption("댐 제원 조회중...");
            SplashScreenManager.Default.SetWaitFormDescription("Searching...");

            try
            {
                WamisAPIService apiService = new WamisAPIService();
                DataTable dtDam = new DataTable();
                dtDam = apiService.getList("mn_dammain", "");
                this.ultraGrid1.DataSource = dtDam; ;
                //WamisAPIController apiController = new WamisAPIController();

                //DataTable dtDam = new DataTable();
                //dtDam = apiController.Get_mn_dammain_All_DataTable("mn_dammain");
                //this.ultraGrid1.DataSource = dtDam;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                SplashScreenManager.CloseForm(false);
            }
        }

        private void frmService_Test_Load(object sender, EventArgs e)
        {

        }
    }
}
