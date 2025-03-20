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
    public partial class frmModel_ModelSGMT : Form
    {
        public Global _global { get; set; }
        private List<ModelSGMT> listCollect_ModelSGMT {  get; set; }

        public frmModel_ModelSGMT()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelSGMT_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelSGMT = new List<ModelSGMT>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_Model":
                    ReadResult_ModelSGMT();
                    break;
            }
        }

        private void ReadResult_ModelSGMT()
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
                        List<ModelSGMT> listFileResult = new List<ModelSGMT>();
                        listFileResult = BizFileIO.ReadModelSGMT(file);

                        DataInsert(listFileResult);

                        this.listCollect_ModelSGMT.AddRange(listFileResult);
                    }
                }

                this.ultraGrid1.DataSource = this.listCollect_ModelSGMT;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(List<ModelSGMT> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_sg_mt (model_date, sgcode, spi_7, spi_30, spi_90, spi_180, spi_270, spi_365, spi_7_dr, spi_30_dr, spi_90_dr, spi_180_dr, spi_270_dr, spi_365_dr) VALUES ");

                int i = 0;
                foreach (ModelSGMT item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, item.spi_7, item.spi_30, item.spi_90, item.spi_180, item.spi_270, item.spi_365
                        , item.spi_7_dr, item.spi_30_dr, item.spi_90_dr, item.spi_180_dr, item.spi_270_dr, item.spi_365_dr));
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
