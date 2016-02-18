using System;
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

        public Line() { }

        public Line(Conversion c)
        {
            ModifiedColumns = new Dictionary<string, bool>();
            LineId = c.LineId;
            StatusId = c.StatusId;
            ProductCodeId = c.ProductCodeId;
            SystemId = c.SystemId;
            ConversionId = c.ConversionId;
        }
    }

    public class Lines
    {
        public List<LineTx> lines { get; set; }
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
            SELECT l.[LineId]
                  ,l.[UnitId]
                  ,l.[LineNumber]
                  ,l.[Stamp]
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
                lines = labdb.Fetch<LineTx, Status, System, ProductCode, LineTx>(LineTx.Map,
                    _lineload + " order by l.LineNumber, l.UnitId");
                systems = labdb.Fetch<System>(System._active);
            }
        }
    }

    public class LineView
    {
        public List<LineTx> timeline { get; set; }

        public Line line { get; set; }
        public DateTime when { get; set; }
        public string action { get; set; }
        public List<System> systems { get; set; }
        public List<Status> statuses { get; set; }
        public List<ProductCode> products { get; set; }
        public List<Conversion> conversions { get; set; }

        public SelectList productList { get; set; }
        public SelectList systemList { get; set; }

        public LineView() { }
        public LineView(int lineid, DateTime stamp)
        {
            if (lineid == 0)
                return;

            when = stamp;
            using (var db = new labDB())
            {
                line = db.Fetch<Line>(Lines._lineload + " where l.lineid = @0", lineid).SingleOrDefault();
                systems = db.Fetch<System>(string.Format(System._attime, stamp.ToString("yyyy-MM-dd HH:mm:ss")));
                products = db.Fetch<ProductCode>(" order by productcode, productspec");

                timeline = LineTx.TimeLine(lineid);

                statuses = db.Fetch<Status>();
            }
            productList = new SelectList(products, "ProductCodeId", "CodeSpec");

            //statusList = new SelectList(statuses, "StatusId", "")
        }
    }
}