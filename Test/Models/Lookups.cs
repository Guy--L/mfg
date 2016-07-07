using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace Test.Models
{
    partial class Line
    {
        private static string _lookup = @"
            select substring ('ABCD', unitid, 1)+'-'+convert(char(1), linenumber) dname, lineid from line
        ";

        private static Dictionary<string, int> _lines;
        private static Dictionary<int, string> _names;
       
        [ResultColumn] public string DName { get; set; }

        public static Dictionary<string, int> all {
            get
            {
                _lines = _lines??Fetch(_lookup).ToDictionary(k => k.DName, v => v.LineId);
                return _lines;
            }
        }

        public static Dictionary<int, string> names
        {
            get
            {
                _names = _names ?? Fetch(_lookup).ToDictionary(v => v.LineId, k => k.DName);
                return _names;
            }
        }
    }

    partial class ProductCode
    {
        private static Dictionary<string, int> _products;

        public static string spec(string _code, string _spec)
        {
            if (_code.Contains('X'))
                _spec = "MS";
            return _code + "-" + (_spec ?? "");   
        }

        public static Dictionary<string, int> all {
            get
            {
                _products = _products ?? Fetch("").ToDictionary(k => spec(k._ProductCode, k.ProductSpec), v => v.ProductCodeId);
                return _products;
            }
        }
    }

    partial class Extruder
    {
        private static Dictionary<string, string> map = new Dictionary<string, string>()
        {
            { "Red", "red" },
            { "Green", "grn" },
            { "Orange", "org" },
            { "Black", "blk" },
            { "Blue", "blue" },
            { "White", "wht" }
        };

        private static Dictionary<string, int> _extruders;
        public static Dictionary<string, int> all
        {
            get
            {
                _extruders = _extruders??Fetch("").Select(e => {
                        var c = e.Color.Split(' ');
                        if (c.Length == 1)
                            e.Color = e.Color.ToLower();
                        else
                            e.Color = map[c[0]] + '/' + map[c[1]];
                        return e;
                    }).ToDictionary(k => k.Color, v => v.ExtruderId);

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
                _systems = _systems??Fetch("").ToDictionary(k => k._System, v => v.SystemId);
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
                _solutions = _solutions??Fetch("").ToDictionary(k => k.SolutionType, v => v.SolutionRecipeId);
                return _solutions;
            }
        }
    }
}
