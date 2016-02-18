using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Html;
using NPoco;
using Omu.ValueInjecter;

namespace Test.Models
{
    public partial class ProductCode
    {
        [ResultColumn] public DateTime Scheduled { get; set; }
        [ResultColumn] public string LineName { get; set; }

        public static string Pending = @"
        ";

        public HtmlString FullCode { get { return new HtmlString(string.Format("{0,8} {1}", _ProductCode.Trim(), ProductSpec?.Trim()??"")); } }
        public HtmlString CodeSpec { get {
                return new HtmlString(string.Format("{0,8} {1}  \t........\t{2}<{3}<{4}\t{5}<{6}<{7}\t{8}<{9}<{10}",
                    _ProductCode.Trim(), ProductSpec?.Trim()??"",
                    ReelMoist_Min, ReelMoist_Aim, ReelMoist_Max,
                    Gly_Min, Gly_Aim, Gly_Max,
                    Oil_Min, Oil_Aim, Oil_Max)); } }

        public static Dictionary<int, ProductCode> products;

        public static void Cache(IEnumerable<int> ids)
        {
            using (labDB d = new labDB())
            {
                products = d.Fetch<ProductCode>(" where productcodeid in (" + string.Join(",", ids) + ")").ToDictionary(i => i.ProductCodeId, v => v);
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

        public bool Exists()
        {
            if (ProductCodeId != 0)
                return false;

            using (labDB d = new labDB())
                return d.Fetch<ProductCode>(" where productcode = @0 and productspec = @1", _ProductCode, ProductSpec).Any();
        }
    }

    public class Products
    {
        public List<ProductCode> products { get; set; }

        public static string _all = @"
          SELECT c.[ConversionId]
                  ,c.[Scheduled]
                  ,l.[Stamp]
                  ,l.[LineId]
                  ,l.[UnitId]
                  ,l.[LineNumber]
                  ,u.[Unit]
                  ,u.[Unit]+cast(l.[LineNumber] as char) LineName
                  ,p.[ProductCodeId]
                  ,p.[ProductCode]
                  ,p.[ProductSpec]
                  ,p.[IsActive]
                  ,p.[PlastSpec]
                  ,p.[WetLayflat_Aim]
                  ,p.[WetLayflat_Min]
                  ,p.[WetLayflat_Max]
                  ,p.[Glut_Aim]
                  ,p.[Glut_Max]
                  ,p.[Glut_Min]
                  ,p.[ReelMoist_Aim]
                  ,p.[ReelMoist_Min]
                  ,p.[ReelMoist_Max]
                  ,p.[LF_Aim]
                  ,p.[LF_Min]
                  ,p.[LF_Max]
                  ,p.[LF_LCL]
                  ,p.[LF_UCL]
                  ,p.[OilType]
                  ,p.[Oil_Aim]
                  ,p.[Oil_Min]
                  ,p.[Oil_Max]
                  ,p.[Gly_Aim]
                  ,p.[Gly_Min]
                  ,p.[Gly_Max]
              from [dbo].[ProductCode] p
              left join [dbo].[Line] l on p.ProductCodeId = l.ProductCodeId
              left join [dbo].[Unit] u on u.UnitId = l.UnitId
              left join [dbo].[Conversion] c on p.ProductCodeId = c.ProductCodeId
              left join [dbo].[Line] n on c.LineId = n.LineId
              order by CASE WHEN l.lineid IS NULL THEN 1 ELSE 0 END, l.lineid
        ";

        public static string _pending = _all + @"
              where c.LineId = @0 and c.Started is null
              order by c.Scheduled
        ";

        public Products()
        {
            using (var labdb = new labDB())
            {
                products = labdb.Fetch<ProductCode>(_all);
            }
        }
    }

    public class ProductView
    {
        public ProductCode p { get; set; }

        public ProductView() { }
        public ProductView(int id)
        {
            using (var db = new labDB())
            {
                p = id > 0 ? db.SingleOrDefaultById<ProductCode>(id) : new ProductCode() { ProductCodeId = 0 };
            }
        }

        public ProductView(ProductCode q)
        {
            p = new ProductCode();
            p.InjectFrom(q);
            p.ProductCodeId = 0;
            p.ProductSpec = "";
        }
    }
}