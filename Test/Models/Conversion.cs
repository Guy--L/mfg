using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using NPoco;
using Omu.ValueInjecter;
using System.Diagnostics;

namespace Test.Models
{
    public partial class Conversion
    {
        public string System { get { return Models.System.Systems[SystemId]; } }
        public string SolutionType { get { return SolutionRecipe.Solutions[SolutionRecipeId]; } }
        public string Color { get { return Extruder.Colors[ExtruderId]; } }
        public Line line { get; set; }
        public ProductCode product { get; set; }

        public string Icon { get { return StatusId==0?"":Status.legend[StatusId]; } }

        public Conversion() { }

        public Conversion(int id)
        {
            if (id != 0)
            {
                var c = new Conversion();
                using (labDB d = new labDB())
                {
                    c = d.SingleById<Conversion>(id);
                }
                this.InjectFrom(c);
                return;
            }
            Note = "";
            ProductCodeId = 0;
            Scheduled = DateTime.Now.AddDays(1);
        }

        public string State
        {
            get
            {
                var now = DateTime.Now;
                var completed = Completed < now;
                var started = Started < now && !completed;
                var ignored = Started > now.AddYears(200) && completed;
                if (ignored) return "ignored";
                if (started) return "started";
                if (completed) return "completed";
                return "created";
            }
        }

        /// <summary>
        /// T-SQL maxdate and DateTime.MaxValue are ever so slightly different
        /// </summary>
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

        public string Complete(int person)
        {
            try {
                var now = DateTime.Now;
                var line = new Line(this);
                if (Started > now.AddYears(200))
                    Started = now;
                Completed = now;
                line.Stamp = now;
                line.PersonId = person;
                line.Update();
                Update();
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
            return "Line conversion completed";
        }

        public string Start(int person)
        {
            try
            {
                var now = DateTime.Now;
                var line = new Line(this);
                Started = now;
                line.StatusId = Status.statuses["Conversion"];
                line.Stamp = now;
                line.PersonId = person;
                line.Update();
                Update();
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
            return "Line conversion started";
        }

        public string Ignore(int person)
        {
            Completed = DateTime.Now;
            Started = DateTime.MaxValue.AddMilliseconds(-1);
            Update();
            return "Conversion ignored";
        }
    }

    public class Conversions
    {
        public List<Conversion> conversions { get; set; }

        public static string _rank = @"
              from (select [lineid],[ProductCodeId],[ConversionId],[Scheduled],[Started],[Completed],[FinishFootage],[StatusId],[ExtruderId],[SystemId],[SolutionRecipeId],[Note],
	                RANK() OVER (PARTITION BY lineid ORDER BY scheduled DESC) as rn from conversion) c
        ";

        public static string _completed = @"
              from (select [lineid],[ProductCodeId],[ConversionId],[Scheduled],[Started],[Completed],[FinishFootage],[StatusId],[ExtruderId],[SystemId],[SolutionRecipeId],[Note],
	                RANK() OVER (PARTITION BY lineid ORDER BY [dbo].SinceNow([Started], [Completed]) ASC) as rn from conversion) c
        ";

        public static string _all = @"
          SELECT {1} c.[ConversionId],c.[SystemId],c.[SolutionRecipeId],c.[ExtruderId],c.[Scheduled],c.[Started],c.[Completed],c.[FinishFootage],c.[StatusId],c.[Note]
                  ,c.[Started]
				  ,s.[System]
                  ,r.[SolutionType]
                  ,e.[Color]
                  ,l.[Stamp] ,l.[LineId] ,l.[UnitId] ,l.[LineNumber]
                  ,p.[ProductCodeId] ,p.[ProductCode] ,p.[ProductSpec] ,p.[PlastSpec] ,p.[WetLayflat_Aim] ,p.[WetLayflat_Min] ,p.[WetLayflat_Max] 
                  ,p.[Glut_Aim] ,p.[Glut_Max] ,p.[Glut_Min] ,p.[ReelMoist_Aim] ,p.[ReelMoist_Min] ,p.[ReelMoist_Max] ,p.[LF_Aim] ,p.[LF_Min] ,p.[LF_Max] ,p.[LF_LCL] ,p.[LF_UCL]
                  ,p.[OilType] ,p.[Oil_Aim] ,p.[Oil_Min] ,p.[Oil_Max] ,p.[Gly_Aim] ,p.[Gly_Min] ,p.[Gly_Max]
                  ,s.[SystemId]
                  ,r.[SolutionRecipeId]
              {0}
              join [dbo].[Line] l on c.LineId = l.LineId
              left join [dbo].[Extruder] e on e.ExtruderId = c.ExtruderId
              left join [dbo].[ProductCode] p on p.ProductCodeId = c.ProductCodeId
              left join [dbo].[System] s on s.SystemId = c.SystemId
              left join [dbo].[SolutionRecipe] r on r.SolutionRecipeId = c.SolutionRecipeId
        ";

        public static string _recent = string.Format(_all, _completed, "") + @"
            where c.rn = 1
            order by [dbo].SinceNow(c.[Started], c.[Completed]) asc
        ";

        public static string _byline = string.Format(_all, "from [dbo].[Conversion] c", "") + @"
            where c.LineId = @0 and c.Started is null
            order by c.Scheduled
        ";

        public static string _undoconversion = string.Format(_all, "from [dbo].[Conversion] c", "top 2") + @"
            where l.LineId = {0} and c.ConversionId <= {1}
            order by [dbo].SinceNow(c.[Started], c.[Completed]) asc
        ";

        public static string _pending = string.Format(_all, _rank, "") + @"
            where c.Completed > dateadd(year, 200, getdate()) or (c.Completed <= getdate() and c.rn < 4) 
            order by c.Scheduled
        ";

        public Conversions(string query)
        {
            using (var labdb = new labDB())
            {
                conversions = labdb.Fetch<Conversion, Line, ProductCode, Conversion>(
                    (c, l, p) =>
                    {
                        c.product = p ?? 
                        new ProductCode()
                        {
                            _ProductCode = "00?00",
                            ProductCodeId = 0
                        };
                        c.ProductCodeId = l.ProductCodeId;
                        p.ProductCodeId = l.ProductCodeId;
                        c.product.ProductCodeId = l.ProductCodeId;
                        c.line = l;
                        c.Future();
                        return c;
                    },
                    query);
            }
        }
    }

    public class UndoConversionView
    {
        public Conversion prior { get; set; }
        public Conversion current { get; set; }

        public UndoConversionView() { }

        public UndoConversionView(int id)
        {
            var c = new Conversion(id);
            var query = string.Format(Conversions._undoconversion, c.LineId, c.ConversionId);
            var u = new Conversions(query);

            current = u.conversions.First();
            current.ProductCodeId = current.product.ProductCodeId;
            prior = null;
            if (u.conversions.Count() > 1)
            {
                prior = u.conversions.Last();
                prior.ProductCodeId = prior.product.ProductCodeId;
                prior.LineId = prior.line.LineId;
            }
        }

        public string Commit(int personid)
        {
            try {
                current.ModifiedColumns = new Dictionary<string, bool>();
                current.Started = DateTime.MaxValue.AddMilliseconds(-100);
                current.Completed = DateTime.MaxValue.AddMilliseconds(-100);
                current.Update();

                var line = new Line(prior);
                line.PersonId = personid;
                line.Stamp = DateTime.Now;
                line.Update();
            }
            catch(Exception e)
            {
                var msg = "Error: " + e.Message;
#if DEBUG
                msg += "\n" + e.StackTrace;
#endif
                return msg;
            }
            return "Conversion reverted";
        }
    }

    public class ConversionView
    {
        public Conversion c { get; set; }
        public List<System> systems { get; set; }
        public List<Status> statuses { get; set; }
        public SelectList products { get; set; }
        public SelectList recipes { get; set; }
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
                    Started = DateTime.MaxValue.AddMilliseconds(-1),
                    Completed = DateTime.MaxValue.AddMilliseconds(-1)
                };
                if (c != null || id > 0)
                    c.Future();
                systems = db.Fetch<System>(System._active);
                statuses = db.Fetch<Status>();
                products = new SelectList(db.Fetch<ProductCode>(" order by productcode, productspec"), "ProductCodeId", "CodeSpec", c.ProductCodeId);
                recipes = new SelectList(db.Fetch<SolutionRecipe>(), "SolutionRecipeId", "SolutionType", c.SolutionRecipeId);
                extruders = new SelectList(db.Fetch<Extruder>(), "ExtruderId", "Color", c.ExtruderId);
            }
        }
    }
}