using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Test.Models
{
    public partial class Parameter
    {
        private static string _icon = @"<i class='fa fa-2x fa-{0}' title='{1} Sample'></i>";

        public static Dictionary<int, Parameter> Types;
        public static Dictionary<string, int> TypeOf;
        public static Dictionary<int, List<int>> Times;

        public static string IconML(int id)
        {
            var p = Types[id];
            return string.Format(_icon, p.Icon, p.Name);
        }

        public string TypeName
        {
            get
            {
                return Regex.Replace(Name, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
            }
        }

        public int[] factor()
        {
            var scale = Scale.Split(',').Select(i => int.Parse(i));
            if (scale.Count() == 1)
            {
                var scale1 = scale.First();
                scale = Enumerable.Range(1, Reading.CountPerRecord).Select(r => scale1);
            }
            return scale.Select(s => (int)Math.Pow(10, s)).ToArray();
        }

        static Parameter()
        {
            using (labDB db = new labDB())
            {
                Types = db.Fetch<Parameter>().ToDictionary(k => k.ParameterId, v => v);
                TypeOf = Types.ToDictionary(k => k.Value.Name, v => v.Key);
                Times = Types.ToDictionary(k => k.Key, k => k.Value.Diary.Split(',').Select(h => int.Parse(h)).ToList());
            }
        }
    }
}
