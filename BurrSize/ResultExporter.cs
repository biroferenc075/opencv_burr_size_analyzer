using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurrSize
{
    public class ResultExporter
    {
        private string fpath;
        private int cnt = 1;
        public ResultExporter(string _fpath)
        {
            fpath = _fpath;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(@fpath)) 
            {
                if (File.Exists(fpath))
                {
                    var sheets = package.Workbook.Worksheets.ToList();
                    foreach (var sh in sheets)
                    {
                        package.Workbook.Worksheets.Delete(sh);
                    }
                }
                package.Workbook.Worksheets.Add("results");
                package.Save();
            }
        }

        public void SaveResults(List<float> burrSizes, string sheetName)
        {
            using (var package = new ExcelPackage(@fpath))
            {
                var sheet = package.Workbook.Worksheets.Add(sheetName);
                int i = 1;
                foreach(var item in burrSizes)
                {
                    sheet.Cells[i++, 1].Value = item;
                    package.Workbook.Worksheets[0].Cells[cnt++, 1].Value = item;
                }

                sheet.Cells[1, 3].Formula = String.Format("MAX(A1:A{0})", i);
                sheet.Cells[2, 3].Formula = String.Format("MIN(A1:A{0})", i);
                sheet.Cells[3, 3].Formula = String.Format("AVERAGE(A1:A{0})", i);
                sheet.Cells[4, 3].Formula = String.Format("MEDIAN(A1:A{0})", i);
                sheet.Cells[5, 3].Formula = String.Format("MODE(A1:A{0})", i);
                sheet.Cells[6, 3].Formula = String.Format("STDEV(A1:A{0})", i);
                sheet.Cells[7, 3].Formula = String.Format("PERCENTILE(A1:A{0},0.10)", i);
                sheet.Cells[8, 3].Formula = String.Format("PERCENTILE(A1:A{0},0.90)", i);

                sheet.Cells[1, 4].Value = "Max";
                sheet.Cells[2, 4].Value = "Min";
                sheet.Cells[3, 4].Value = "Avg";
                sheet.Cells[4, 4].Value = "Median";
                sheet.Cells[5, 4].Value = "Mode";
                sheet.Cells[6, 4].Value = "Stdev";
                sheet.Cells[7, 4].Value = "10th Perc";
                sheet.Cells[8, 4].Value = "90th Perc";

                var chart = sheet.Drawings.AddHistogramChart("chart");
                chart.SetPosition(0, 0, 5, 0);
                chart.SetSize(1200, 800);
                chart.Title.Text = sheetName + " burr sizes";
                chart.Series.Add(sheet.Cells[String.Format("$A$1:$A${0}",i)]);
                chart.Series.First().Fill.Color = System.Drawing.Color.Black;
                chart.Series.First().Binning.Count = 15;
                package.Save();
            }
        }
        public void AggregateResults()
        {
            using (var package = new ExcelPackage(@fpath))
            {
                var sheet = package.Workbook.Worksheets[0];

                var chart = sheet.Drawings.AddHistogramChart("chart");
                chart.SetPosition(0, 0, 5, 0);
                chart.SetSize(1200, 800);
                chart.Title.Text = "Burr Sizes";
                
                chart.Series.Add(sheet.Cells[String.Format("$A$1:$A${0}", cnt)]);
                chart.Series.First().Fill.Color = System.Drawing.Color.Black;
                chart.Series.First().Binning.Size = 10;
                sheet.Cells[1, 3].Formula = String.Format("MAX(A1:A{0})", cnt);
                sheet.Cells[2, 3].Formula = String.Format("MIN(A1:A{0})", cnt);
                sheet.Cells[3, 3].Formula = String.Format("AVERAGE(A1:A{0})", cnt);
                sheet.Cells[4, 3].Formula = String.Format("MEDIAN(A1:A{0})", cnt);
                sheet.Cells[5, 3].Formula = String.Format("MODE(A1:A{0})", cnt);
                sheet.Cells[6, 3].Formula = String.Format("STDEV(A1:A{0})", cnt);
                sheet.Cells[7, 3].Formula = String.Format("PERCENTILE(A1:A{0},0.10)", cnt);
                sheet.Cells[8, 3].Formula = String.Format("PERCENTILE(A1:A{0},0.90)", cnt);

                sheet.Cells[1, 4].Value = "Max";
                sheet.Cells[2, 4].Value = "Min";
                sheet.Cells[3, 4].Value = "Avg";
                sheet.Cells[4, 4].Value = "Median";
                sheet.Cells[5, 4].Value = "Mode";
                sheet.Cells[6, 4].Value = "Stdev";
                sheet.Cells[7, 4].Value = "10th Perc";
                sheet.Cells[8, 4].Value = "90th Perc";

                package.Save();
            }
        }
    }
}
