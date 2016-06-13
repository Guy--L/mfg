using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            SELECT p.[PlanId]
                  ,p.[Stamp]
                  ,p.[LineId]
                  ,p.[ProductCodeId]
                  ,p.[Code]
                  ,p.[Spec]
                  ,p.[ExtruderId]
                  ,p.[Solution]
                  ,p.[SystemId]
                  ,p.[SolutionRecipeId]
                  ,p.[ConversionStatus]
                  ,p.[Comment]
              FROM [dbo].[Plan] p
        ";

        private static string _byline = @" where lineid = {0}";
        private static string _prior = _byline + @" and stamp < '{1}'";
        private static string _byday = _byline + @" and datediff(dd, stamp, '{1}') = 0";

        private static string _recent = _plan + @"
            left join [dbo].[Plan] q
            on (p.LineId = q.LineId and p.stamp < q.stamp)
            where q.LineId is null and p.LineId in ({0})
        ";

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
            "moved to",
            "(converted on saturday)",
            "down for pm"
        };

        private static Dictionary<string, int> frequency = skip.ToDictionary(s => s, v => 0);
        private static int nullcount = 0;
        private static int mismatches = 0;

        private int AppendFt(string comment)
        {
            var footage = comment.ToLower().IndexOf(" ft");
            if (footage > 0)
            {
                var start = comment.Substring(0, footage).LastIndexOf(' ');
                var feet = comment.Substring(start + 1, footage - start).Replace(",", "");
                int len = 0;
                if (!int.TryParse(feet, out len))
                {
                    var tst = 1;
                }
                FinishFootage = len;
            }

            return Append(comment);
        }

        private int Append(string comment)
        {
            var prior = Comment ?? "";
            prior = prior.Length == 0 ? "" : prior + " ";
            Comment = prior + comment;
            Save();
            return 1;
        }

        public static void Comments(ISheet sheet, int column, int startrow, List<int> lines)
        {
            if (lines.Count == 0)
                return;

            List<Plan> plans = null;
            var list = string.Join(",", lines.Select(i => i.ToString()).ToArray());
            using (labDB d = new labDB())
            {
                plans = d.Fetch<Plan>(string.Format(_recent, list));
            }
            if (plans.Count == 0)
                return;

            int row = startrow;
            ICell cell;
            IRow r = sheet.GetRow(row);
            if (r == null)
                return;

            ICell comment = r.GetCell(column - 1);
            bool incomment = false;

            List<string> comments = new List<string>();
            StringBuilder buffer = new StringBuilder();

            while (row < sheet.LastRowNum)
            {
                comment = sheet.GetRow(row).GetCell(column - 1);
                if (comment != null && comment.StringCellValue.Contains("*"))
                {
                    startrow = row;
                    incomment = true;
                    break;
                }
                row++;
            }

            while (row < sheet.LastRowNum)
            {
                comment = sheet.GetRow(row).GetCell(column - 1);
                cell = sheet.GetRow(row++).GetCell(column);

                if (comment != null && comment.StringCellValue.Contains("*"))
                {
                    if (row > startrow + 1)
                    {
                        comments.Add(buffer.ToString());
                        buffer.Clear();
                    }
                    incomment = true;
                }

                if (cell != null && cell.CellType == CellType.String)
                {
                    if (incomment) buffer.Append(cell.StringCellValue.Trim() + " ");
                }
                else
                    incomment = false;
            }
            if (buffer.Length > 0)
                comments.Add(buffer.ToString());

            foreach (var cmt in comments)
            {
                var parse = cmt;
                var hyphen = parse.IndexOf("-");
                int edits = 0;
                while (hyphen >= 0)
                {
                    var line = parse.Substring(hyphen - 1).Split(' ')[0].Replace(":","");
                    int lineid = 0;
                    if (!Line.all.TryGetValue(line, out lineid))
                        break;

                    edits = plans.Where(p => p.LineId == lineid).Select(p => p.AppendFt(cmt)).Sum();

                    parse = parse.Substring(hyphen + line.Length);
                    hyphen = parse.IndexOf("-");
                }
                if (edits > 0) continue;

                var soln = cmt.IndexOf("SS#");
                if (soln >= 0)
                {
                    var solnum = cmt.Substring(soln + 3).Split(' ')[0].Replace(",", "");
                    if (solnum == "")
                        solnum = cmt.Substring(soln + 3).Split(' ')[1].Replace(",", "");

                    var system = System.all[solnum];

                    edits = plans.Where(p => p.SystemId == system).Select(p => p.Append(cmt)).Sum();

                    if (edits == 0)
                    {
                        Console.WriteLine("-->old system " + cmt);
                        // need to go back to find lines where system is going down
                    }
                    else continue;
                }

                plans.Select(p => p.Append(cmt)).Sum();
            }
        }

        public static int Parse(int lineid, DateTime stamp, ISheet sheet, int column, int startrow)
        {
            int row = startrow;
            ICell cell;
            ICell comment;

            while (true) {
                comment = sheet.GetRow(row).GetCell(column - 1);
                cell = sheet.GetRow(row++).GetCell(column);

                if (cell == null || cell.CellType != CellType.String) return row;
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

                if (startrow + 1 == row)
                {
                    Console.WriteLine(cell.StringCellValue);
                    if (!plan.IsConsistent(lineid, stamp))
                        Console.WriteLine(lineid + " not consistent on " + stamp.ToStamp());
                    continue;
                }
                plan.LineId = lineid;
                plan.Stamp = stamp;
                plan.Save();
                prior[lineid] = plan;
            }
        }

        public Plan() { }

        public Plan(string value)
        {
            string[] s = value.Replace('.',',').Split(',');
            if (s.Count() > 2) s[1] = s[1] + s[2];              // ignore second comma in plan
            if (s.Count() < 2)
            {
                int n = 3;
                var r = value.TakeWhile(c => (n -= (c == ' ' ? 1 : 0)) > 0).Count();
                StringBuilder revalue = new StringBuilder(value);
                revalue[r] = ',';
                s = revalue.ToString().Split(',');
            }

            var p = s[0].Split('-');
            var code = p[0].Trim();
            var spec = p[1].Trim().Replace('#', ' ');

            int product = 0;
            ProductCodeId = ProductCode.all.TryGetValue(ProductCode.spec(code, spec), out product) ? product : 0;
            Code = code;
            Spec = spec;

            var d = s[1].Replace("( ","(").Replace("("," (").Replace("  ", " ").Trim().Split(' ');

            int sysi = 1;
            int coli = 2;

            if (d.Length == 2 && d[0].Trim()[0] == '(')
            {
                sysi = 0;
                coli = 1;
            }
            if (d.Length == 4 && d[1] == "(ESM)")
            {
                sysi = 2;
                coli = 3;
            }
            if (d[0] == "-")
            {
                d[0] = d[1];
                sysi = 2;
                coli = 3;
            }
            if (d[1] == "")
            {
                sysi = 2;
                coli = 3;
            }

            var system = d[sysi].Substring(d[sysi].IndexOf('#') + 1).Replace(")", "");
            SystemId = System.all[system];

            var color = d[coli].Substring(1, d[coli].Length - 2).ToLower().Replace("//", "/");
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
