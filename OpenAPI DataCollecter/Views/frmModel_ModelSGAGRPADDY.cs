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
    public partial class frmModel_ModelSGAGRPADDY : Form
    {
        public Global _global { get; set; }
        private List<ModelSGAGRPADDY> listCollect_ModelSGAGRPADDY { get; set; }

        public frmModel_ModelSGAGRPADDY()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelSGAGRPADDY_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelSGAGRPADDY = new List<ModelSGAGRPADDY>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_Model":
                    ReadResult_ModelSGAGRPADDY();
                    break;
            }
        }

        private void ReadResult_ModelSGAGRPADDY()
        {
            string filePath = Path.Combine(Application.StartupPath, "ModelSGMT");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {
                        List<ModelSGAGRPADDY> listFileResult = new List<ModelSGAGRPADDY>();
                        listFileResult = BizFileIO.ReadModelSGAGRPADDY(file);

                        DataInsert(listFileResult);

                        this.listCollect_ModelSGAGRPADDY.AddRange(listFileResult);
                    }
                }
                this.ultraGrid1.DataSource = this.listCollect_ModelSGAGRPADDY;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(List<ModelSGAGRPADDY> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_sg_agr_paddy (model_date, sgcode, srsi_a, srsi_a_drt) VALUES ");

                int i = 0;
                foreach (ModelSGAGRPADDY item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, item.SRSI_A, item.SRSI_A_drt));
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
