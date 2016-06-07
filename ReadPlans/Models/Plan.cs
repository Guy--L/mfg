using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace ReadPlans.Models
{
    static class Extensions
    {
        public static string ToStamp(this DateTime stamp)
        {
            return stamp.ToString("yyyy-MM-dd");
        }

        public static string Before(this DateTime stamp)
        {
            return stamp.ToStamp() + " 00:00:00";
        }
    }

    partial class Plan
    {
        private static Dictionary<int, Plan> prior = new Dictionary<int, Plan>();

        private static string _plan = @"
            SELECT [PlanId]
                  ,[Stamp]
                  ,[LineId]
                  ,[ProductCodeId]
                  ,[Code]
                  ,[Spec]
                  ,[ExtruderId]
                  ,[Solution]
                  ,[SystemId]
                  ,[SolutionRecipeId]
                  ,[ConversionStatus]
              FROM [dbo].[Plan]
        ";

        private static string _byline = @" where lineid = {0}";
        private static string _prior = _byline + @" and stamp < '{1}'";
        private static string _byday = _byline + @" and datediff(dd, stamp, '{1}') = 0";

        private static string _remove = @"
            delete from [dbo].[Plan]
            where datediff(dd, stamp, '{0}') = 0
            and lineid not in ({1})
        ";

        private static List<string> skip = new List<string>()
        {
            "to",
            "down",
            "no",
            "conversion",
            "down for repair",
            "remain",
            "exempt", 
            "exempt moved to",
            "monday",
            "tuesday",
            "wednesday",
            "thursday",
            "friday",
            "saturday",
            "will remain",
            "running",
            "moved to"
        };

        private static Dictionary<string, int> frequency = skip.ToDictionary(s => s, v => 0);
        private static int nullcount = 0;
        private static int mismatches = 0;
         
        public static int Parse(int lineid, DateTime stamp, ISheet sheet, int column, int startrow)
        {
            int row = startrow;
            ICell cell;
            ICell comment;

            while (true) {
                comment = sheet.GetRow(row).GetCell(column - 1);
                cell = sheet.GetRow(row++).GetCell(column);

                if (cell.CellType != CellType.String) return row;
                var value = cell.StringCellValue.Trim();
                if (skip.Contains(value.ToLower()))
                {
                    frequency[value.ToLower()]++;
                    continue;
                }
                if (string.IsNullOrEmpty(value)) return row;
                if (comment != null &&
                    comment.CellType == CellType.String && 
                    comment.StringCellValue.Contains('*')) return row;

                var plan = new Plan(cell.StringCellValue);

                if (startrow+1 == row)
                {
                    Console.WriteLine(cell.StringCellValue);
                    if (!plan.IsConsistent(lineid, stamp))
                        Console.WriteLine(lineid + " not consistent on " + stamp.ToStamp());
                }
                else
                {
                    plan.LineId = lineid;
                    plan.Stamp = stamp;
                    plan.Save();
                    prior[lineid] = plan;
                }
            }
        }

        public Plan() { }

        public Plan(string value)
        {
            var s = value.Replace('.',',').Split(',');
            if (s.Count() > 2) s[1] = s[1] + s[2];              // ignore second comma in plan

            var p = s[0].Split('-');
            var code = p[0].Trim();
            var spec = p[1].Trim().Replace('#', ' ');

            int product = 0;
            ProductCodeId = ProductCode.all.TryGetValue(ProductCode.spec(code, spec), out product) ? product : 0;
            Code = code;
            Spec = spec;

            var d = s[1].Replace(")(", ") (").Replace("  ", " ").Trim().Split(' ');

            var system = d[1].Substring(d[1].IndexOf('#') + 1).Replace(")", "");
            SystemId = System.all[system];

            var color = d[2].Substring(1, d[2].Length - 2).ToLower().Replace("//", "/");
            int extruder = 0;
            ExtruderId = Extruder.all.TryGetValue(color, out extruder)? product: 0;

            var solution = 0;
            SolutionRecipeId = SolutionRecipe.all.TryGetValue(d[0], out solution) ? solution : 0;
        }

        public Plan(int line, DateTime stamp)
        {
            SingleOrDefault(string.Format(_byline, line));
        }

        public bool IsConsistent(int line, DateTime stamp)
        {
            Plan test = null;
            if (!prior.TryGetValue(line, out test))
            {
                test = Fetch(string.Format(_prior, line, stamp.Before())).LastOrDefault();
                prior[line] = test;
            }
            var same = test != null
                && test.ExtruderId == ExtruderId
                && test.SystemId == SystemId
                && test.SolutionRecipeId == SolutionRecipeId
                && test.ProductCodeId == ProductCodeId;

            if (!same)
            {
                Console.WriteLine();
                if (test == null)
                {
                    nullcount++;
                    Console.WriteLine(LineId + " is null");
                }
                else
                {
                    mismatches++;
                    test.Print();
                }
                Print();
            }
            return same;
        }

        public static void Stats()
        {
            var format = "{0, -"+ (skip.Max(s => s.Length) + 1)+"}{1}";

            foreach (var e in frequency)
            {
                Console.WriteLine(string.Format(format, e.Key, e.Value));
            }
            Console.WriteLine();
            Console.WriteLine("null priors " + nullcount);
            Console.WriteLine("mismatched priors " + mismatches);

            var all = Fetch("");
            var noproducts = all.Where(p => p.ProductCodeId == 0).Count();
            var nosolution = all.Where(p => p.SolutionRecipeId == 0).Count();

            Console.WriteLine("product codes not found " + noproducts);
            Console.WriteLine("solutions not found " + nosolution);
        }

        public void Print()
        {
            Console.WriteLine(LineId + ": " + ExtruderId + " " + SystemId + " " + SolutionRecipeId + " " + ProductCodeId + " " + Code + " " + Spec);
        }

        /// <summary>
        /// Remove all line plans not in list
        /// </summary>
        /// <param name="lines">List of lines in plan</param>
        /// <param name="stamp">Date of plans</param>
        public static void Sweep(List<int> lines, DateTime stamp)
        {
            if (lines.Count == 0)
                return;

            var list = string.Join(",", lines.Select(i => i.ToString()).ToArray());
       
            using (labDB d = new labDB())
            {
                d.Execute(string.Format(_remove, stamp.ToStamp(), list));
            }
        }
    }
}
