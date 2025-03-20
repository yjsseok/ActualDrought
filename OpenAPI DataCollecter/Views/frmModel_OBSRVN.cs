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
    public partial class frmModel_OBSRVN : Form
    {
        public Global _global { get; set; }
        private List<SoilMoisture> listCollect_Soil { get; set; }

        public frmModel_OBSRVN()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_OBSRVN_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_Soil = new List<SoilMoisture>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult":
                    ReadResult_Soil();
                    break;
                case "btnSaveDatabase":

                    break;
            }
        }

        private void ReadResult_Soil()
        {
            string filePath = Path.Combine(Application.StartupPath, "Soil");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {
                        List<SoilMoisture> listFileResult = new List<SoilMoisture>();
                        listFileResult = BizFileIO.ReadSoilMoisture(file);

                        DataInsert(listFileResult);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool DataInsert(List<SoilMoisture> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_obsrvn_soil_mst (measuredt, sitecode, wc10, wc20, wc30, wc40, wc50, bat) VALUES ");

                int i = 0;
                foreach (var item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7})", item.measureDT.ToString("yyyy-MM-dd HH:mm:ss"), item.SiteCode, item.wc10, item.wc20, item.wc30, item.wc40, item.wc50, item.bat));
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
    }
}
