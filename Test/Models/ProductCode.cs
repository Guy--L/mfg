using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Html;

namespace Test.Models
{
    public partial class ProductCode
    {
        public static string Pending = @"
        ";

        public HtmlString CodeSpec { get {
                return new HtmlString(string.Format("{0,8} {1}  \t........\t{2}<{3}<{4}\t{5}<{6}<{7}\t{8}<{9}<{10}", 
                    _ProductCode.Trim(), ProductSpec.Trim(),
                    ReelMoist_Min, ReelMoist_Aim, ReelMoist_Max,
                    Gly_Min, Gly_Aim, Gly_Max,
                    Oil_Min, Oil_Aim, Oil_Max)); } }

        public static Dictionary<int, ProductCode> products;

        public static void Cache(IEnumerable<int> ids)
        {
            using (labDB d = new labDB())
            {
                products = d.Fetch<ProductCode>(" where productcodeid in ("+string.Join(",", ids)+")").ToDictionary(i => i.ProductCodeId, v => v);
            }
        }

        public string MoistSpec
        {
            get { return ReelMoist_Min?.ToString("N2") + " < " + ReelMoist_Aim?.ToString("N2") + " < " + ReelMoist_Max?.ToString("N2"); }
        }

        public string GlySpec
        {
            get { return Gly_Min?.ToString("N2") + " < " + Gly_Aim?.ToString("N2") + " < " + Gly_Max?.ToString("N2"); }
        }
    }
}