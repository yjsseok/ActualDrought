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
    public partial class frmModel_ModelSGHYD : Form
    {
        public Global _global { get; set; }
        private List<ModelSGHYD> listCollect_ModelSGHYD {  get; set; }

        public frmModel_ModelSGHYD()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelSGHYD_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelSGHYD = new List<ModelSGHYD>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_Model":
                    ReadResult_ModelSGHYD();
                    break;
            }
        }

        private void ReadResult_ModelSGHYD()
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
                        List<ModelSGHYD> listFileResult = new List<ModelSGHYD>();
                        listFileResult = BizFileIO.ReadModelSGHYD(file);

                        DataInsert(listFileResult);

                        this.listCollect_ModelSGHYD.AddRange(listFileResult);
                    }
                }

                this.ultraGrid1.DataSource = this.listCollect_ModelSGHYD;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(List<ModelSGHYD> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_sg_hyd (model_date, sgcode, srsi_h, srsi_h_drt) VALUES ");

                int i = 0;
                foreach (ModelSGHYD item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, item.SRSI_H, item.SRSI_H_drt));
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
