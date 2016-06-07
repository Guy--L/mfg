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
        static int count = 0;

        static void Main(string[] args)
        {
            var inputdir = ConfigurationManager.AppSettings["input"];

            count = 0;
            foreach (var year in args)
            {
                var input = Path.Combine(inputdir, year);

                DirectoryInfo info = new DirectoryInfo(input);
                var filesin = info.GetFiles();
                var names = filesin.Select(f => f.Name.Replace("Schedule", "").Trim()).ToLookup(k => k.Substring(0,6));
                var finals = names.Select(s => s.Reverse().Skip(s.Count() > 1 ? 1 : 0).Take(1).Single()).ToList();

                foreach (var file in finals)
                {
                    count++;
                    GetPlans(Path.Combine(input, "Schedule "+file));
                    //Console.Write("\r{0} read, {1} weeks         ", file, count);
                }
            }
            Plan.Stats();
            Console.ReadKey();
        }

        static void GetPlans(string xls)
        {
            if (!File.Exists(xls))
                xls = xls.Replace(" ", "");

            using (FileStream file = new FileStream(xls, FileMode.Open, FileAccess.Read))
            {
                IWorkbook wb = WorkbookFactory.Create(file);
                ISheet sh = wb.GetSheet("Schedule");

                int weekday = 0;
                var dateRow = sh.GetRow(3);
                var dateCell = dateRow.GetCell(weekday * 2 + 1);

                var lines = new List<int>();
                while (dateCell != null && dateCell.CellType != CellType.Blank)
                {
                    DateTime stamp = dateCell.DateCellValue;
                    
                    var lineIndex = 5;
                    var lineCell = sh.GetRow(lineIndex).GetCell(weekday*2);

                    while (lineCell != null && lineCell.CellType != CellType.Blank)
                    {
                        int lineid = 0;

                        if (lineCell.StringCellValue.Contains('*')) break;

                        if (Line.all.TryGetValue(lineCell.StringCellValue, out lineid))
                            lines.Add(lineid);
                        else
                        {
                            do
                            {
                                lineIndex++;
                                dateCell = sh.GetRow(lineIndex).GetCell(weekday * 2 + 1);
                                stamp = dateCell.DateCellValue;
                            }
                            while (dateCell != null && stamp == null);
                            lineIndex += 2;
                            lineCell = sh.GetRow(lineIndex).GetCell(weekday * 2);
                            continue;
                        }
                        Console.Write(lineCell.StringCellValue + " " + stamp.ToStamp() + " ");
                        lineIndex = Plan.Parse(lineid, stamp, sh, weekday * 2 + 1, lineIndex) + 1;
                        lineCell = sh.GetRow(lineIndex).GetCell(weekday * 2);
                    }
                    Plan.Sweep(lines, stamp);
                    lines.Clear();

                    weekday++;
                    dateCell = dateRow.GetCell(weekday * 2 + 1);
                }
            }
        }
    }
}
