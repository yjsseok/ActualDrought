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

namespace Service.DataCollect.Dam
{
    public partial class frmConfig : Form
    {
        public Global _global { get; set; }

        public frmConfig()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            this.dtpStart.Value = DateTime.Now.AddDays(-5);
            this.dtpEnd.Value = DateTime.Now;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (SetConfig() == true)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Save Error!");
                this.Close();
            }
        }

        private bool SetConfig()
        {
            try
            {
                _global.startDate = this.dtpStart.Value;
                _global.endDate = this.dtpEnd.Value;
                _global.PeriodUse = this.chkPeriod.Checked;

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
