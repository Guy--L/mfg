using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;
using Test.Properties;

namespace Test.Models
{
    public partial class TensileTest
    {
        public TensileTest() { }

        [ResultColumn] public int TestNumber { get; set; }
        [ResultColumn] public int ProductCode { get; set; }
        [ResultColumn] public bool Passed { get; set; }
    }

    public class TensileTests
    {
        public Sample sample { get; set; }
        public List<Reading> list { get; set; }

        public TensileTests(int id)
        {
            using (labDB db = new labDB())
            {
                sample = db.Single<Sample>(string.Format(Resources.TensileSampleById, id));
                list = db.Fetch<TensileTest>(string.Format(Resources.TensileTestBySample, id));
            }
        }
    }

    public class TensileTestView
    {
        public TensileTest t { get; set; }
        public Sample sample { get; set; }

        public int read1 { get; set; }
        public int read2 { get; set; }
        public int read3 { get; set; }
        public int read4 { get; set; }

        public TensileTestView()
        {
            t = new TensileTest();
            sample = new Sample();
        }

        public TensileTestView(int id)
        {
            using (labDB db = new labDB())
            {
                if (id > 0)
                {
                    t = db.SingleOrDefaultById<TensileTest>(id);
                }
                else
                {
                    t = new TensileTest()
                    {
                        SampleId = -id,
                        Stamp = DateTime.Now
                    };
                }
            }
        }
    }
}
