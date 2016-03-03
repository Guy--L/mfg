using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Test.Properties;

namespace Test.Models
{
    public class linemap
    {
        public int line { get; set; }
        public int lineid { get; set; }
    }

    public partial class Unit
    {
        private static string _map = @"
            select cast (UnitId as char(1)) + cast (LineNumber as char(1)) as line, lineid from line
        ";

        public static string[] ips;
        public static Dictionary<int, int> lineids = null;
        public static Dictionary<int, string> id2line = null;

        public Unit() { }
        public Unit(int id)
        {
            UnitId = id;
            _Unit = Code(id);
        }

        public static string Code(int id)
        { return "0ABCDEFGH"[id].ToString(); } 

        public static List<Line> Lines(int unit)
        {
            using (labDB db = new labDB())
            {
                return db.Fetch<Line>(Resources.LinesByUnit, unit);
            }
        }

        public static void setLineIds()
        { 
            using (labDB db = new labDB())
            {
                var lmap = db.Fetch<linemap>(_map);
                lineids = lmap.ToDictionary(k => k.line, v => v.lineid);
                id2line = lmap.ToDictionary(k => k.lineid, v => Code(v.line / 10) + v.line % 10);
            }
        }
    }
}
