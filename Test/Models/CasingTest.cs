using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;
using Test.Properties;

namespace Test.Models
{
    public class OilStat
    {
        public static decimal Slope { get; set; }
        public static decimal Intercept { get; set; }
    }

    public partial class CasingTest
    {
        [ResultColumn] public int LineIndex { get; set; }
        [ResultColumn] public int System { get; set; }

        public decimal? PctMoist { get { return (1 - (DryWt / WetWt)); } }
        public decimal? GlyDry { get { return DryWt / WetWt * GlyWetWt; } }
        public decimal? PctOil { get { return (OilStat.Slope * OilArea + OilStat.Intercept) / (GlyDry * 20000); } }
        public decimal? PctGly { get { return GlyArea / (2 * GlySTD * GlyDry * (1 - PctOil)); } }
    }

    public class CasingTests
    {
        public List<CasingTest> list { get; set; }

        public CasingTests(int id)
        {
            using (labDB d = new labDB())
            {
                list = d.Fetch<CasingTest>(" where casinggroupid = @0", id);
            }
        }
    }

    public class CasingTestView
    {
        public CasingTestView() { }

        public CasingTestView(int id)
        {
            if (id == 0)
            {
                t = new CasingTest()
                {
                    CasingTestId = 0,
                    DateTime = DateTime.Now
                };
            }
            else
            {
                using (labDB db = new labDB())
                    t = db.SingleOrDefault<CasingTest>(" where CasingTestId = @0", id);
            }
        }
        
        public void Save()
        {
            t.Save();
        }

        public CasingTest t { get; set; }
    }
}