using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infragistics.Win.UltraWinGrid.DocumentExport;
using Infragistics.Win.UltraWinGrid.ExcelExport;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.Printing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace UFRI.FramWork
{
    public static class InfragisticsLib
    {
        private static UltraGridExcelExporter _excelExporter;
        private static UltraGridDocumentExporter _pdfExporter;
        private static UltraGridPrintDocument _printDocument;
        private static UltraPrintPreviewDialog _ppDlg;

        public static bool ExcelExporter(UltraGrid grid, string SaveName)
        {
            try
            {
                _excelExporter = new UltraGridExcelExporter();
                _excelExporter.ExportAsync(grid, SaveName);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //public static bool ExcelExporter(UltraGrid grid, string SaveName)
        //{
        //    try
        //    {
        //        string rootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EXPORT");

        //        if (!Directory.Exists(rootPath))
        //        {
        //            if (MessageBox.Show("디렉토리가 존재하지 않습니다. 새로 생성하시겠습니까?", "생성", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
        //            {
        //                Directory.CreateDirectory(rootPath);
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }

        //        _excelExporter = new UltraGridExcelExporter();
        //        string TempName = DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + SaveName + ".csv";

        //        _excelExporter.ExportAsync(grid, Path.Combine(rootPath, TempName));

        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// UltraGrid의 내용을 Excel로 Export시켜주는 함수
        /// </summary>
        /// <param name="grid">UltraGrid</param>
        /// <param name="exportPath">Export할 위치</param>
        /// <returns>성공여부</returns>
        public static bool ExcelExporter(UltraGrid grid)
        {
            try
            {
                string rootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EXPORT");

                if (!Directory.Exists(rootPath))
                {
                    if (MessageBox.Show("디렉토리가 존재하지 않습니다. 새로 생성하시겠습니까?", "생성", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                    {
                        Directory.CreateDirectory(rootPath);
                    }
                    else
                    {
                        return false;
                    }
                }

                SaveFileDialog savedlg = new SaveFileDialog();

                savedlg.InitialDirectory = rootPath;
                savedlg.Filter = "Excel files (*.xls)|*.xls|All files (*.*)|*.*";
                savedlg.FilterIndex = 1;
                savedlg.RestoreDirectory = true;

                if (savedlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (savedlg.FileName != string.Empty)
                    {
                        _excelExporter = new UltraGridExcelExporter();
                        _excelExporter.ExportAsync(grid, savedlg.FileName);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// UltraGrid의 내용을 PDF로 Export시켜주는 함수
        /// </summary>
        /// <param name="grid">UltraGrid</param>
        /// <param name="exportPath">Export할 위치</param>
        /// <returns>성공여부</returns>
        public static bool PDFExporter(UltraGrid grid)
        {
            try
            {
                string rootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EXPORT");

                if (!Directory.Exists(rootPath))
                {
                    if (MessageBox.Show("디렉토리가 존재하지 않습니다. 새로 생성하시겠습니까?", "생성", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                    {
                        Directory.CreateDirectory(rootPath);
                    }
                    else
                    {
                        return false;
                    }
                }

                SaveFileDialog savedlg = new SaveFileDialog();

                savedlg.InitialDirectory = rootPath;
                savedlg.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                savedlg.FilterIndex = 1;
                savedlg.RestoreDirectory = true;

                if (savedlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (savedlg.FileName != string.Empty)
                    {
                        _pdfExporter = new UltraGridDocumentExporter();
                        _pdfExporter.ExportAsync(grid, savedlg.FileName, GridExportFileFormat.PDF);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 프린트 화면
        /// </summary>
        /// <param name="grid">UltraGrid</param>
        /// <returns>성공여부</returns>
        public static bool PrintExporter(UltraGrid grid)
        {
            try
            {
                _printDocument = new UltraGridPrintDocument();
                _ppDlg = new UltraPrintPreviewDialog();

                _printDocument.Grid = grid;
                _ppDlg.Document = _printDocument;

                _ppDlg.ShowDialog();

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
