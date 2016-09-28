using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public partial class Extruder
    {
        [Ignore] public static Dictionary<int, string> Colors; 

        static Extruder()
        {
            Colors = repo.Fetch<Extruder>().ToDictionary(k => k.ExtruderId, v => v.Color);
        }
    }
}