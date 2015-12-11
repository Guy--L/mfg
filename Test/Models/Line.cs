using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Test.Models
{
    public partial class Line
    {
        private static string letters = " ABCDEFGHIJ";  // units start at 1

        public Status status { get; set; }
        public System system { get; set; }
        public ProductCode product { get; set; }

        public string Name
        {
            get { return letters[UnitId] + LineNumber.ToString(); }
        }
    }

    public class Lines
    {
        public List<Line> lines { get; set; }
        public List<System> systems { get; set; }

        public static string _linehistory = @"
              FROM (
                select [Stamp]
                    ,[LineId]
                    ,[UnitId]
                    ,[LineNumber]
                    ,[StatusId]
                    ,[ProductCodeId]
                    ,[SystemId]
                    ,rank() over (partition by LineId order by Stamp) operations
                from [dbo].[Line]
              ) l 
        ";

        public static string _lineload = @"
            SELECT l.[Stamp]
                  ,l.[LineId]
                  ,l.[UnitId]
                  ,l.[LineNumber]
                  ,i.[StatusId]
                  ,i.[Code]
                  ,i.[Icon]
                  ,i.[Color]
                  ,s.[SystemId]
                  ,s.[System]
                  ,p.[ProductCodeId]
                  ,p.[ProductCode]
                  ,p.[ProductSpec]
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
              FROM [dbo].[Line] l
              left join [dbo].[Status] i on i.StatusId = l.StatusId
              left join [dbo].[ProductCode] p on p.ProductCodeId = l.ProductCodeId
              left join [dbo].[System] s on s.SystemId = l.SystemId
        ";

        public Lines()
        {
            using (var labdb = new labDB())
            {
                lines = labdb.Fetch<Line, Status, System, ProductCode, Line >(
                    (l, s, y, p) =>
                    {
                        l.status = s;
                        l.system = y;
                        l.product = p ?? new ProductCode() { _ProductCode = "00?00", ProductCodeId = 0 };
                        return l;
                    },
                    _lineload + " order by l.LineNumber, l.UnitId");
                systems = labdb.Fetch<System>(System.Active);
            }
        }
    }

    public class LineView
    {
        public Line line { get; set; }
        public List<System> systems { get; set; }
        public List<ProductCode> products { get; set; }
        public List<Status> statuses { get; set; }
        public List<Conversion> conversions { get; set; }

        public SelectList productList { get; set; }
        public SelectList systemList { get; set; }

        public LineView(int lineid)
        {
            if (lineid == 0)
                return;

            using (var db = new labDB())
            {
                line = db.Fetch<Line>(Lines._lineload + " where l.lineid = @0", lineid).SingleOrDefault();
                systems = db.Fetch<System>(System.Current);
                products = db.Fetch<ProductCode>(" order by productcode, productspec");
                conversions = db.Fetch<Conversion>(" where lineid = @0", lineid);
                statuses = db.Fetch<Status>();
            }
            productList = new SelectList(products, "ProductCodeId", "CodeSpec");

            //statusList = new SelectList(statuses, "StatusId", "")
        }
    }
}