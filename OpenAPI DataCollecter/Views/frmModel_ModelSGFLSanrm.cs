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

namespace OpenAPI_DataCollecter
{
    public partial class frmModel_ModelSGFLSanrm: Form
    {
        public Global _global { get; set; }
        private List<ModelSGFLSanrm> listCollect_ModelSGFLSanrm { get; set; }
        public frmModel_ModelSGFLSanrm()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelSGFLSanrm_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelSGFLSanrm = new List<ModelSGFLSanrm>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_Model":
                    ReadResult_ModelSGFLSanrm();
                    break;
            }
        }

        private void ReadResult_ModelSGFLSanrm()
        {
            string filePath = Path.Combine(Application.StartupPath, "Anomaly");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {
                        List<ModelSGFLSanrm> listFileResult = new List<ModelSGFLSanrm>();
                        listFileResult = BizFileIO.ReadModelSGFLSanrm(file);

                        DataInsert(listFileResult);

                        this.listCollect_ModelSGFLSanrm.AddRange(listFileResult);
                    }
                }

                this.ultraGrid1.DataSource = this.listCollect_ModelSGFLSanrm;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(List<ModelSGFLSanrm> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_sg_fls_anrm (model_date, sgcode, precipitation, evaporation, soilmoisture) VALUES ");

                int i = 0;
                foreach (ModelSGFLSanrm item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3}, {4})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, item.precipitation, item.evaporation, item.soilMoisture));
                    i++;
                }

                var command = new NpgsqlCommand(query.ToString(), _global.NpgSQLconn);
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
