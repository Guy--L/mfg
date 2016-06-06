using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using ReadPlans.Models;

namespace ReadPlans
{
    class Program
    {
        static DateTimeFormatInfo formatProvider;

        static void Main(string[] args)
        {
            formatProvider = new DateTimeFormatInfo();
            formatProvider.Calendar.TwoDigitYearMax = DateTime.Now.Year;

            var inputdir = ConfigurationManager.AppSettings["input"];

            foreach (var year in args)
            {
                var input = Path.Combine(inputdir, year);

                DirectoryInfo info = new DirectoryInfo(input);
                FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
                foreach (FileInfo file in files)
                {
                    GetPlans(file.FullName);
                }
            }
        }

        static void GetPlans(string xls)
        {
            using (FileStream file = new FileStream(xls, FileMode.Open, FileAccess.Read))
            {
                IWorkbook wb = WorkbookFactory.Create(file);
                ISheet sh = wb.GetSheet("Schedule");

                int weekday = 1;
                var dateRow = sh.GetRow(4);
                var dateCell = dateRow.GetCell(weekday * 2);

                while (dateCell.CellType != CellType.Blank)
                {
                    var lines = new List<int>();
                    DateTime stamp = DateTime.ParseExact(dateCell.StringCellValue, "MM/dd/yy", formatProvider).AddHours(2);
                    
                    var lineIndex = 6;
                    var lineCell = sh.GetRow(lineIndex).GetCell(weekday*2-1);

                    while (lineCell.CellType != CellType.Blank)
                    {
                        var lineid = Line.all[lineCell.StringCellValue];
                        lines.Add(lineid);

                        lineIndex = Plan.Parse(lineid, stamp, sh, weekday * 2, lineIndex);
                        lineCell = sh.GetRow(lineIndex).GetCell(weekday * 2 - 1);
                    }
                    Plan.Sweep(lines, stamp);

                    weekday++;
                    dateCell = dateRow.GetCell(weekday * 2);
                }

            }

        }

    }
}
