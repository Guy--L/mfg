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

        public static int Parse(int lineid, DateTime stamp, ISheet sheet, int column, int startrow)
        {
            int row = startrow;
            ICell cell;

            while (true) {
                cell = sheet.GetRow(row++).GetCell(column);
                if (cell.CellType != CellType.String) return row;
                var value = cell.StringCellValue.Trim();
                if (value.ToLower() == "to" || value.ToLower() == "down") continue;
                if (string.IsNullOrEmpty(value)) return row;
                var plan = new Plan(cell.StringCellValue);
                if (startrow == row)
                {
                    if (!plan.IsConsistent(lineid, stamp))
                        Console.WriteLine(lineid + " not consistent on " + stamp.ToStamp());
                }
                else
                {
                    plan.LineId = lineid;
                    plan.Stamp = stamp;
                    plan.Save();
                }
            }
        }

        public Plan() { }

        public Plan(string value)
        {
            var s = value.Split(',');
            var p = s[0].Split('-');
            var code = p[0].Trim();
            var spec = p[1].Trim().Replace('#', ' ');

            ProductCodeId = ProductCode.all.SingleOrDefault(r => r._ProductCode == code && r.ProductSpec == spec).ProductCodeId;

            var d = s[1].Trim().Split(' ');
            var q = d[1].Split(')');

            var system = q[0].Substring(q[0].IndexOf('#') + 1);
            SystemId = System.all[system];

            var color = q[1].Substring(2, q[1].Length - 2).ToLower();
            ExtruderId = Extruder.all[color];

            SolutionRecipeId = SolutionRecipe.all[d[0]];
        }

        public Plan(int line, DateTime stamp)
        {
            SingleOrDefault(string.Format(_byline, line));
        }

        public bool IsConsistent(int line, DateTime stamp)
        {
            var test = SingleOrDefault(string.Format(_prior, line, stamp.Before()));
            return test.ExtruderId == ExtruderId
                && test.SystemId == SystemId
                && test.SolutionRecipeId == SolutionRecipeId
                && test.ProductCodeId == ProductCodeId;
        }

        /// <summary>
        /// Remove all line plans not in list
        /// </summary>
        /// <param name="lines">List of lines in plan</param>
        /// <param name="stamp">Date of plans</param>
        public static void Sweep(List<int> lines, DateTime stamp)
        {
            var list = string.Join(",", lines.Select(i => i.ToString()).ToArray());
       
            using (labDB d = new labDB())
            {
                d.Execute(string.Format(_remove, stamp.ToStamp(), list));
            }
        }
    }
}
