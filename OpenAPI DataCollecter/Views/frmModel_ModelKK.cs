using Npgsql;
using OpenAPI.Controls;
using OpenAPI.Model;
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

namespace OpenAPI_DataCollecter
{
    public partial class frmModel_ModelKK : Form
    {
        public Global _global { get; set; }
        private List<ModelKK> listCollect_ModelKK { get; set; }

        public frmModel_ModelKK()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelKK_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelKK = new List<ModelKK>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_ModelKK":
                    ReadResult_ModelKK();
                    break;
                case "btnSaveDatabase":
                    //if (DataInsert(this.listCollect_ModelKK) == true)
                    //{

                    //}
                    //else
                    //{

                    //}
                    break;
            }
        }

        private bool DataInsert(List<ModelKK> listCollect_ModelKK)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_kk (model_date, code_sg, prcp, spi1, sm, sm_rda, index_ssmi1) VALUES ");

                int i = 0;
                foreach (var item in listCollect_ModelKK)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3}, {4}, {5}, {6})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, item.PCP, item.SPI1, item.SM, item.SM_RDA, item.SSMI1));
                    i++;
                }

                var command = new NpgsqlCommand(query.ToString(), _global.NpgSQLconn);
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                GMLogHelper.WriteLog($"StackTrace : {ex.StackTrace}");
                GMLogHelper.WriteLog($"Message : {ex.Message}");

                return false;
            }
        }

        private void ReadResult_ModelKK()
        {
            string filePath = Path.Combine(Application.StartupPath, "each_data_daily");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {
                        List<ModelKK> listFileResult = new List<ModelKK>();
                        listFileResult = BizFileIO.ReadModelKK(file);

                        DataInsert(listFileResult);

                        this.listCollect_ModelKK.AddRange(listFileResult);
                    }
                }

                this.ultraGrid1.DataSource = this.listCollect_ModelKK;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
