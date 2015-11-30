using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public partial class OilSTD
    {
        public static Dictionary<int, double> intercept;
        public static Dictionary<int, double> slope;

        public static void compile(List<OilSTD> records)
        {
            if (slope == null) slope = new Dictionary<int, double>();
            if (intercept == null) intercept = new Dictionary<int, double>();

            var calibs = records.Select(r => r.CasingGroupId).Distinct();

            foreach(var c in calibs) {
                if (slope.ContainsKey(c.Value))
                    continue;

                var points = records.Where(r => r.CasingGroupId == c);

                var Ex = points.Sum(p => p.Area);
                var Ey = points.Sum(p => p.Concentration);
                var Exy = points.Sum(p => p.Area * p.Concentration);
                var Ex2 = points.Sum(p => p.Area * p.Area);

                var n = points.Count();
                double? a = (n * Exy - Ex * Ey) / (n * Ex2 - Ex * Ex);
                double? b = (Ey - a.Value * Ex) / n;

                slope.Add(c.Value, a.Value);
                intercept.Add(c.Value, b.Value);
            }
        }
    }
}