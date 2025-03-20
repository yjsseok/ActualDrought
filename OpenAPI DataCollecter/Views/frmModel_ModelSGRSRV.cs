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
    public partial class frmModel_ModelSGRSRV : Form
    {
        public Global _global { get; set; }
        private List<ModelSGRSRV> listCollect_ModelSGRSRV { get; set; }

        public frmModel_ModelSGRSRV()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelSGRSRV_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelSGRSRV = new List<ModelSGRSRV>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_Model":
                    ReadResult_ModelSGRSRV();
                    break;
            }
        }

        private void ReadResult_ModelSGRSRV()
        {
            string filePath = Path.Combine(Application.StartupPath, "RSRV");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {
                        List<ModelSGRSRV> listFileResult = new List<ModelSGRSRV>();
                        listFileResult = BizFileIO.ReadModelSGRSRV(file);

                        DataInsert(listFileResult);

                        this.listCollect_ModelSGRSRV.AddRange(listFileResult);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(List<ModelSGRSRV> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_sg_rsrv_daily (model_date, sgcode, storage_sum) VALUES ");

                int i = 0;
                foreach (ModelSGRSRV item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, item.storageSum));
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
