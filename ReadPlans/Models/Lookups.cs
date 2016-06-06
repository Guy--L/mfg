using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadPlans.Models
{
    partial class Line
    {
        private static string _lookup = @"
            select substring ('ABCD', unitid, 1)+'-'+convert(char(1), linenumber) name, lineid from line
        ";

        private static Dictionary<string, int> _lines;

        public string Name { get; set; }

        public static Dictionary<string, int> all {
            get
            {
                if (_lines == null)
                    _lines = Fetch(_lookup).ToDictionary(k => k.Name, v => v.LineId);
                return _lines;
            }
        }
    }

    partial class ProductCode
    {
        private static List<ProductCode> _products;
        public static List<ProductCode> all {
            get
            {
                if (_products == null)
                    _products = Fetch("");
                return _products;
            }
        }
    }

    partial class Extruder
    {
        private static Dictionary<string, string> map = new Dictionary<string, string>()
        {
            { "Green", "grn" },
            { "Orange", "org" },
            { "Black", "blk" },
            { "Blue", "blu" },
            { "White", "wht" }
        };

        private static Dictionary<string, int> _extruders;
        public static Dictionary<string, int> all
        {
            get
            {
                if (_extruders == null)
                {
                    _extruders = Fetch("").Select(e =>
                        {
                            var c = e.Color.Split(' ');
                            if (c.Length == 1)
                                e.Color = e.Color.ToLower();
                            else
                                e.Color = map[c[0]] + '/' + map[c[1]];
                            return e;
                        }).ToDictionary(k => k.Color, v => v.ExtruderId);
                }
                return _extruders;
            }
        }
    }

    partial class System
    {
        private static Dictionary<string, int> _systems;
        public static Dictionary<string, int> all
        {
            get
            {
                if (_systems == null)
                    _systems = Fetch("").ToDictionary(k => k._System, v => v.SystemId);
                return _systems;
            }
        }
    }

    partial class SolutionRecipe
    {
        private static Dictionary<string, int> _solutions;
        public static Dictionary<string, int> all
        {
            get
            {
                if (_solutions == null)
                    _solutions = Fetch("").ToDictionary(k => k.SolutionType, v => v.SolutionRecipeId);
                return _solutions;
            }
        }
    }
}
