using DevExpress.XtraSplashScreen;
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
    public partial class frmModel_ModelSGFLS: Form
    {
        public Global _global { get; set; }
        public List<ModelSGFLS> listCollect_ModelSGFLS { get; set; }
        public List<ModelSGFLS> listCollect_ModelSGFLS_Mon { get; set; }

        public frmModel_ModelSGFLS()
        {
            InitializeComponent();
            _global = Global.GetInstance();
        }

        private void frmModel_ModelSGFLS_Load(object sender, EventArgs e)
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            this.listCollect_ModelSGFLS = new List<ModelSGFLS>();
            this.listCollect_ModelSGFLS_Mon = new List<ModelSGFLS>();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "btnReadResult_Model":
                    ReadResult_ModelSGFLS();
                    break;
                case "btnMonitor":
                    ReadResult_ModelSGFLS_Monitor();
                    break;
                case "btnMerge":
                    if (this.listCollect_ModelSGFLS.Count > 0 && this.listCollect_ModelSGFLS_Mon.Count > 0)
                    {
                        MergeData();
                    }
                    break;
                case "btnSaveDatabase":
                    if (DataInsert(this.listCollect_ModelSGFLS) == true)
                    {
                        MessageBox.Show("성공");
                    }
                    else
                    {
                        MessageBox.Show("실패");
                    }
                    break;
            }
        }

        private bool DataInsert(List<ModelSGFLS> listFileResult)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("INSERT INTO drought.tb_model_sg_fls (model_date, sgcode, stvi, eddi_spi, flashdroughtmonitor) VALUES ");

                int i = 0;
                foreach (ModelSGFLS item in listFileResult)
                {
                    if (i != 0)
                    {
                        query.Append(" , ");
                    }

                    double STVI = 0.0;
                    if (item.STVI == double.NaN)
                    {
                        STVI = 0.0;
                    }
                    else
                    {
                        STVI = item.STVI;
                    }

                    double EDDI_SPI = 0.0;
                    if (item.EDDI_SPI == double.NaN)
                    {
                        EDDI_SPI = 0.0;
                    }
                    else
                    {
                        EDDI_SPI = item.EDDI_SPI;
                    }

                    query.Append(string.Format("('{0}', '{1}', {2}, {3}, {4})", item.modelDate.ToString("yyyy-MM-dd"), item.sgCode, STVI, EDDI_SPI, item.FlashDroughtMonitor));
                    i++;
                }

                var command = new NpgsqlCommand(query.ToString(), _global.NpgSQLconn);
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                string Error = ex.Message;
                return false;
            }

        }
        private void MergeData()
        {
            SplashScreenManager.ShowForm(typeof(frmWait));
            SplashScreenManager.Default.SetWaitFormCaption("매칭중...");
            SplashScreenManager.Default.SetWaitFormDescription("Searching...");

            int i = 0;
            foreach (var item in this.listCollect_ModelSGFLS)
            {
                ModelSGFLS monitor = this.listCollect_ModelSGFLS_Mon.Where(a => a.modelDate == item.modelDate && a.sgCode == item.sgCode).SingleOrDefault();

                if (monitor != null)
                {
                    item.FlashDroughtMonitor = monitor.FlashDroughtMonitor;
                }

                if (i % 500 == 0 ) 
                {
                    SplashScreenManager.Default.SetWaitFormDescription(string.Format("{0}/{1} 처리중", i, this.listCollect_ModelSGFLS.Count()));
                }
                

                i++;
            }

            SplashScreenManager.CloseForm(false);

            this.ultraGrid1.DataSource = this.listCollect_ModelSGFLS;
        }

        private void ReadResult_ModelSGFLS_Monitor()
        {
            string filePath = Path.Combine(Application.StartupPath, "Monitor");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {
                        List<ModelSGFLS> listFileResult = new List<ModelSGFLS>();
                        listFileResult = BizFileIO.ReadModelSGFLS_Mon(file);

                        this.listCollect_ModelSGFLS_Mon.AddRange(listFileResult);
                    }
                }

                this.ultraGrid1.DataSource = this.listCollect_ModelSGFLS_Mon;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ReadResult_ModelSGFLS()
        {
            string filePath = Path.Combine(Application.StartupPath, "Indices");
            string filePath_mon = Path.Combine(Application.StartupPath, "Monitor");

            SplashScreenManager.ShowForm(typeof(frmWait));
            SplashScreenManager.Default.SetWaitFormCaption("매칭중...");
            SplashScreenManager.Default.SetWaitFormDescription("Searching...");

            try
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    string fileExtension = Path.GetExtension(file);

                    if (fileExtension.ToUpper() == ".CSV")
                    {                       
                        List<ModelSGFLS> listFileResult_Ind = new List<ModelSGFLS>();
                        List<ModelSGFLS> listFileResult_Mon = new List<ModelSGFLS>();

                        listFileResult_Ind = BizFileIO.ReadModelSGFLS(file);

                        //Monitor읽기
                        string[] subString = Path.GetFileNameWithoutExtension(file).Split('_');
                        string sgcd = subString[3];

                        SplashScreenManager.Default.SetWaitFormDescription(string.Format("{0} 처리중", sgcd));

                        string filename = string.Format("{0}_{1}.csv", "FlashDroughtMonitor", sgcd);
                        string file_mon = Path.Combine(filePath_mon, filename);

                        if (File.Exists(file_mon) == true)
                        {
                            listFileResult_Mon = BizFileIO.ReadModelSGFLS_Mon(file_mon);
                        }

                        if (listFileResult_Ind.Count() > 0 && listFileResult_Mon.Count() > 0)
                        {
                            foreach (var item in listFileResult_Ind)
                            {
                                ModelSGFLS monitor = listFileResult_Mon.Where(a => a.modelDate == item.modelDate && a.sgCode == item.sgCode).SingleOrDefault();

                                if (monitor != null)
                                {
                                    item.FlashDroughtMonitor = monitor.FlashDroughtMonitor;
                                }
                            }
                        }

                        this.listCollect_ModelSGFLS.AddRange(listFileResult_Ind);
                    }
                }

                SplashScreenManager.CloseForm(false);

                this.ultraGrid1.DataSource = this.listCollect_ModelSGFLS;


            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
