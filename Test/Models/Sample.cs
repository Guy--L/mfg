using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPoco;
using System.Reflection;

namespace Test.Models
{
    public class Samples
    {
        private static string _all = @"
        select  t.Sampleid
            ,t.stamp
            ,t.lineid
            ,n.unitid
            ,n.linenumber
            ,t.productcodeid
            ,t.note
            ,t.[Completed]
            ,t.tech
            ,p.productcode
            ,p.productspec
            ,p.productcode+' '+p.productspec as codespec
            ,p.wettensileminimum
            ,(select count(s.readingid) from reading s where t.sampleid = s.SampleId) as TestCount
            from Sample t
            join ProductCode p on t.ProductCodeId = p.ProductCodeId
            join Line n on n.LineId = t.LineId
            ";

        public List<Sample> list { get; set; }

        public Samples()
        {
            list = new List<Sample>();
            using (labDB db = new labDB())
            {
                list = db.Fetch<Sample>(_all);
            }
        }
    }

    public partial class Sample
    {
        protected static string _update = @"
            update sample set @0 = @1 where sampleid = @2
        ";

        private static string _last = @"
            select top 1 nextscheduled 
            from Sample 
            where lineid = @0 and parameterid = @1 
            orderby nextscheduled desc
        ";

        private static string _nextsamples = @"
            ;with cteRowNumber as (
            select parameterid
                    ,nextscheduled
                    ,lineid
                    ,row_number() 
                        over(partition by parameterid order by nextscheduled desc) as RowNum
                from sample
            )
            select parameterid, nextscheduled, lineid
                from cteRowNumber
                where RowNum = 1 and lineid = @0
        ";

        private static string _icon = @"<i class='fa fa-2x fa-{0}' title='{1} Sample'></i>";
        private static string _tcells = @"<td>" + _icon + "</td><td>{1}</td>";
        private static string _sample = @"
              SELECT b.[SampleId]
                  ,b.[Stamp]
                  ,b.[ProductCodeId]
                  ,b.[LineId]
				  ,s.[LineNumber]
                  ,b.[Tech]
                  ,b.[Note]
				  ,b.[Completed]
                  ,r.[ProductCode]
				  ,r.[ProductSpec]
                  ,r.[ProductCode]+r.[ProductSpec] as CodeSpec
				  ,r.[WetTensileMinimum]
				  ,s.[UnitId]
              FROM [dbo].[Sample] b
              join [dbo].[Line] s on b.[LineId] = s.[LineId]
              join [dbo].[ProductCode] r on b.[ProductCodeId] = r.[ProductCodeId]
			  where b.[TensileSampleId] = {0}
        ";

        public Sample() { }
        public Sample(int id)
        {
            SingleOrDefault(id);
        }

        public List<Reading> readings { get; set; }

        [ResultColumn] public string ProductCode { get; set; }
        [ResultColumn] public string CodeSpec { get; set; }
        [ResultColumn] public int TestCount { get; set; }
        [ResultColumn] public int LineNumber { get; set; }
        [ResultColumn] public int UnitId { get; set; }
        [ResultColumn] public float WetTensileMinimum { get; set; }

        public bool Up { get; set; }

        public string Line { get { return Unit.Code(UnitId) + LineNumber; } }
        public string TypeCells
        {
            get
            {
                var type = Reading.Types[ParameterId.Value];
                return string.Format(_tcells, type.Icon, type.TypeName);
            }
        }
        public string Type { 
            get { 
                var type = Reading.Types[ParameterId.Value];
                return string.Format(_icon, type.Icon, type.TypeName); 
            } 
        }

        public bool Active { get { return !Completed.HasValue; } }

        public static DateTime NextSlot(string typeName)
        {
            var t = Reading.TypeOf[typeName];
            return NextSlot(t);
        }

        public static DateTime NextSlot(int type)
        {
            // next will be set to 1:00 even if now is 1:59
            // may want to refine this algorithm either with global or parameter based intervals
            var now = DateTime.Now;
            var diary = Reading.Types[type].Diary.Split(',').Select(h => int.Parse(h)).ToList();
            if (diary.Count == 1 && diary[0] == 0)
                return now;

            var next = diary.FirstOrDefault(h => h >= now.Hour);

            if (next == diary[0])                    // wrap back to prior day
                return new DateTime(now.Year, now.Month, now.Day-1, diary[diary.Count - 1], 0, 0);
            
            return new DateTime(now.Year, now.Month, now.Day, next, 0, 0);
        }

        /// <summary>
        /// Get next available scheduled slot
        /// </summary>
        /// <param name="line">On this line</param>
        /// <param name="type">With this type</param>
        /// <returns></returns>
        public static DateTime NextSample(int line, int type)
        {
            DateTime? done = null;
            using (labDB d = new labDB())
            {
                done = d.ExecuteScalar<DateTime>(_last, line, type);
            }
            return done??NextSlot(type);
        }

        public static List<Sample> NextSamples(int line)
        {
            List<Sample> todo;
            using (labDB d = new labDB())
            {
                todo = d.Fetch<Sample>(_nextsamples, line);
            }
            if (todo.Count == Reading.Types.Count)
                return todo;
            var planned = todo.Select(t => t.ParameterId);
            foreach (var m in Reading.Types.Keys.Where(k => !planned.Contains(k)))
            {
                todo.Add(new Sample() { ParameterId = m, NextScheduled = NextSlot(m), LineId = line });
            }
            return todo;
        }
    }

    public class SampleView
    {
        public Sample t { get; set; }
        public Parameter p { get; set; }
        public SelectList products;
        public int? LineId { get; set; }
        public List<Line> lines;
        public Dictionary<int, Parameter> types;
        public List<Sample> pending;
        public Dictionary<char, int[]> grid;
        public Reading r { get; set; }

        public SampleView()
        {}

        public SampleView(int id)
        {
            grid = Reading.Lines;
            types = Reading.Types;

            using (labDB db = new labDB())
            {
                lines = db.Fetch<Line>();
                var productlist = db.Fetch<ProductCode>("").OrderBy(p=>p.CodeSpec);
                if (id > 0)
                {
                    t = db.SingleOrDefaultById<Sample>(id);
                    LineId = t.LineId;
                    p = types[t.ParameterId.Value];
                }
                else
                {
                    t = new Sample()
                    {
                        Stamp = DateTime.Now,
                        ProductCodeId = 0,
                        LineId = 0,
                        ParameterId = 0
                    };
                    pending = Reading.Types.Keys.Select(k => new Sample() { ParameterId = k }).ToList();
                    LineId = 0;
                }
                products = AddNone(new SelectList(productlist, "ProductCodeId", "CodeSpec", t.ProductCodeId));
            }
        }

        private SelectList AddNone(SelectList list)
        {
            List<SelectListItem> _list = list.ToList();
            _list.Insert(0, new SelectListItem() { Value = "0", Text = "" });
            return new SelectList((IEnumerable<SelectListItem>)_list, "Value", "Text");
        }
    }
}