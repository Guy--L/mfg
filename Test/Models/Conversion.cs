using System;
using System.Collections.Generic;
using System.Web.Mvc;
using NPoco;

namespace Test.Models
{
    public partial class Conversion
    {
        [ResultColumn] public string System { get; set; }
        [ResultColumn] public string SolutionType { get; set; }
        [ResultColumn] public string Code { get; set; }
        [ResultColumn] public string Color { get; set; }
        public Line line { get; set; }
        public ProductCode product { get; set; }

        public void Future()
        {
            var diff = DateTime.MaxValue - Completed;
            Completed = (diff.TotalMinutes < 1) ? DateTime.MaxValue : Completed;

            if (!Started.HasValue) Started = DateTime.MaxValue;
            else
            {
                diff = DateTime.MaxValue - Started.Value;
                Started = (diff.TotalMinutes < 1) ? DateTime.MaxValue : Started;
            }
        }
    }

    public class Conversions
    {
        public List<Conversion> conversions { get; set; }

        public static string _all = @"
          SELECT c.[ConversionId]
                  ,c.[Scheduled]
                  ,c.[Started]
                  ,c.[Completed]
                  ,c.[FinishFootage]
                  ,c.[Exempt]
                  ,r.[SolutionType]
                  ,s.[System]
                  ,x.[ExemptId]
                  ,x.[ExemptCode]
                  ,c.[Note]
                  ,e.[Color] 
                  ,l.[Stamp]
                  ,l.[LineId]
                  ,l.[UnitId]
                  ,l.[LineNumber]
                  ,s.[SystemId]
                  ,r.[SolutionRecipeId]
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
              from [dbo].[Conversion] c
              join [dbo].[Line] l on c.LineId = l.LineId
              left join [dbo].[Extruder] e on e.ExtruderId = c.ExtruderId
              left join [dbo].[ProductCode] p on p.ProductCodeId = c.ProductCodeId
              left join [dbo].[System] s on s.SystemId = c.SystemId
              left join [dbo].[Exempt] x on x.ExemptId = c.ExemptId
              left join [dbo].[SolutionRecipe] r on r.SolutionRecipeId = c.SolutionRecipeId
        ";

        public static string _pending = _all + @"
              where c.LineId = @0 and c.Started is null
              order by c.Scheduled
        ";


        public Conversions()
        {
            using (var labdb = new labDB())
            {
                conversions = labdb.Fetch<Conversion, Line, ProductCode, Conversion>(
                    (c, l, p) =>
                    {
                        c.product = p ?? new ProductCode() { _ProductCode = "00?00", ProductCodeId = 0 };
                        c.line = l;
                        c.Future();
                        return c;
                    },
                    _all);
            }
        }
    }

    public class ConversionView
    {
        public Conversion c { get; set; }
        public List<System> systems { get; set; }
        public SelectList products { get; set; }
        public SelectList recipes { get; set; }
        public SelectList exempt { get; set; }
        public SelectList extruders { get; set; }
        public List<Line> lines { get; set; }

        public ConversionView() { }

        public ConversionView(int id)
        {
            var lns = new Lines();
            lines = lns.lines;

            using (var db = new labDB())
            {
                c = id > 0 ? db.SingleOrDefaultById<Conversion>(id) : new Conversion() {
                    SolutionRecipeId = 0,
                    Started = DateTime.MaxValue,
                    Completed = DateTime.MaxValue
                };
                if (c != null || id > 0)
                    c.Future();
                systems = db.Fetch<System>(System._active);
                products = new SelectList(db.Fetch<ProductCode>(" order by productcode, productspec"), "ProductCodeId", "CodeSpec", c.ProductCodeId);
                exempt = new SelectList(db.Fetch<Exempt>(), "ExemptId", "Code", c.ExemptId ?? 1);
                recipes = new SelectList(db.Fetch<SolutionRecipe>(), "SolutionRecipeId", "SolutionType", c.SolutionRecipeId);
                extruders = new SelectList(db.Fetch<Extruder>(), "ExtruderId", "Color", c.ExtruderId);
            }
        }
    }
}