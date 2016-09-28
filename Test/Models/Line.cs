using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NPoco;

namespace Test.Models
{
    public partial class Line
    {
        [ResultColumn, ComplexMapping] public Unit unit { get; set; }

        private static string letters = " ABCDEFGHIJ";  // units start at 1

        [ResultColumn, ComplexMapping] public Status status { get; set; }
        [ResultColumn, ComplexMapping] public System system { get; set; }
        [ResultColumn, ComplexMapping] public ProductCode product { get; set; }

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
        public List<Line> lines { get; set; }
        public List<System> systems { get; set; }

        public List<List<Line>> table { get; set; }

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
                  ,i.[StatusId]          as status__StatusId
                  ,i.[Code]              as status__Code
                  ,i.[Icon]              as status__Icon
                  ,i.[Color]             as status__Color
                  ,s.[SystemId]          as system__SystemId   
                  ,s.[System]            as system__System 
                  ,p.[ProductCodeId]     as product__ProductCodeId
                  ,p.[ProductCode]       as product__ProductCode     
                  ,p.[ProductSpec]       as product__ProductSpec      
                  ,p.[PlastSpec]         as product__PlastSpec        
                  ,p.[WetLayflat_Aim]    as product__WetLayflat_Aim   
                  ,p.[WetLayflat_Min]    as product__WetLayflat_Min   
                  ,p.[WetLayflat_Max]    as product__WetLayflat_Max   
                  ,p.[Glut_Aim]          as product__Glut_Aim      
                  ,p.[Glut_Max]          as product__Glut_Max     
                  ,p.[Glut_Min]          as product__Glut_Min      
                  ,p.[ReelMoist_Aim]     as product__ReelMoist_Aim 
                  ,p.[ReelMoist_Min]     as product__ReelMoist_Min 
                  ,p.[ReelMoist_Max]     as product__ReelMoist_Max 
                  ,p.[LF_Aim]            as product__LF_Aim        
                  ,p.[LF_Min]            as product__LF_Min        
                  ,p.[LF_Max]            as product__LF_Max        
                  ,p.[LF_LCL]            as product__LF_LCL        
                  ,p.[LF_UCL]            as product__LF_UCL        
                  ,p.[OilType]           as product__OilType       
                  ,p.[Oil_Aim]           as product__Oil_Aim       
                  ,p.[Oil_Min]           as product__Oil_Min       
                  ,p.[Oil_Max]           as product__Oil_Max       
                  ,p.[Gly_Aim]           as product__Gly_Aim       
                  ,p.[Gly_Min]           as product__Gly_Min       
                  ,p.[Gly_Max]           as product__Gly_Max       
              FROM [dbo].[Line] l
              left join [dbo].[Status] i on i.StatusId = l.StatusId
              left join [dbo].[ProductCode] p on p.ProductCodeId = l.ProductCodeId
              left join [dbo].[System] s on s.SystemId = l.SystemId
        ";

        public Lines()
        {
            using (var labdb = new labDB())
            {
                lines = labdb.Fetch<Line>(_lineload + " order by l.LineNumber, l.UnitId");
                systems = labdb.Fetch<System>(System._active);
            }
        }
    }

    public class LinePicker
    {
        public List<string> units { get; set; }
        public List<Line>[] lines { get; set; }

        public LinePicker(Lines lns)
        {
            units = lns.lines.Select(n => Unit.Code(n.UnitId)).Distinct().ToList();
            var rows = lns.lines.Count / units.Count;
            lines = Enumerable.Range(0, rows).Select(r => lns.lines.Where(ln => ln.LineNumber == r + 1).ToList()).ToArray();           
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
                systems = db.Fetch<System>(string.Format(System._attime, stamp.ToStamp()));
                products = db.Fetch<ProductCode>(" order by productcode, productspec");

                timeline = LineTx.TimeLine(lineid);
                statuses = db.Fetch<Status>();
            }
            productList = new SelectList(products, "ProductCodeId", "CodeSpec");

            //statusList = new SelectList(statuses, "StatusId", "")
        }
    }
}