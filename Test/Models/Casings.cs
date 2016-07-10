using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class Casings
    {
        public List<CasingSample> samples { get; set; }

        public Casings(int product)
        {
            using (labDB d = new labDB())
            {
                samples = d.Fetch<CasingSample>(CasingSamplesView._batch + " where productcodeid = @0", product);
            }
        }
    }
}