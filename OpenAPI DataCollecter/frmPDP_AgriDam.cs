using OpenAPI.DataServices;
using OpenAPI.Model;
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
    public partial class frmPDP_AgriDam : Form
    {
        public Global _global { get; set; }

        #region [Delegate]
        delegate void WriteToStatusCallback(string message);    //스레드교착
        #endregion

        public frmPDP_AgriDam()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmPDP_AgriDam_Load(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.dtpStart.Value = new DateTime(1992, 01, 01);
            this.dtpEnd.Value = new DateTime(2024, 12, 31);
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnSearch":
                    Search_AgriDamData();
                    break;
            }
        }

        private void Search_AgriDamData()
        {
            //농업용저수지 데이터 조회
            List<AgriDamSpec> listAgriDam = new List<AgriDamSpec>();
            listAgriDam = NpgSQLService.Get_AgriDamSpec();

            //데이터 요청


        }
    }
}
