﻿using System;
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
                  ,p.[ProductCode]
				  ,p.[ProductSpec]
                  ,p.[ProductCode]+r.[ProductSpec] as CodeSpec
                  ,p.[LF_Aim]
                  ,p.[LF_Min]
                  ,p.[LF_Max]
                  ,p.[LF_LCL]
                  ,p.[LF_UCL]
				  ,p.[WetTensileMinimum]
				  ,s.[UnitId]
                  ,r.[ReadingId]            as readings__ReadingId   
                  ,r.[LineId]               as readings__LineId      
                  ,r.[Stamp]                as readings__Stamp      
                  ,r.[R1]                   as readings__R1          
                  ,r.[R2]                   as readings__R2          
                  ,r.[R3]                   as readings__R3          
                  ,r.[R4]                   as readings__R4          
                  ,r.[R5]                   as readings__R5          
                  ,r.[Average]              as readings__Average     
                  ,r.[ParameterId]          as readings__ParameterId 
                  ,r.[Operator]             as readings__Operator    
                  ,r.[ProductCodeId]        as readings__ProductCodeId
                  ,r.[EditCount]            as readings__EditCount   
                  ,r.[Scheduled]            as readings__Scheduled   
                  ,r.[Reel]                 as readings__Reel    
                  ,r.[Footage]              as readings__Footage     
              FROM [dbo].[Sample] b
              join [dbo].[Line] s on b.[LineId] = s.[LineId]
              join [dbo].[ProductCode] p on b.[ProductCodeId] = p.[ProductCodeId]
              left join [dbo].[Reading] r on r.[SampleId] = b.[SampleId] 
        ";

        public Sample() { }
        public Sample(int sampleid)
        {
            Type = Parameter.Types[Parameter.TypeOf[GetType().Name]];

            if (sampleid == 0)
            {
                Structure();
                return;
            }

            repo.FetchOneToMany<Sample>(p => p.readings, _sample + "where b.[SampleId] = @0", sampleid);
        }

        [ResultColumn] public List<Reading> readings { get; set; }
        public Parameter Type { get; set; }

        [ResultColumn] public int TestCount { get; set; }
        [ResultColumn] public float WetTensileMinimum { get; set; }

        [ResultColumn, ComplexMapping] public Line Line;
        [ResultColumn, ComplexMapping] public ProductCode Product;

        public bool Up { get; set; }

        public string TypeCells
        {
            get
            {
                var type = Parameter.Types[ParameterId.Value];
                return string.Format(_tcells, type.Icon, type.TypeName);
            }
        }
        public string TypeString { 
            get { 
                var type = Parameter.Types[ParameterId.Value];
                return string.Format(_icon, type.Icon, type.TypeName); 
            } 
        }

        public bool Active { get { return !Completed.HasValue; } }

        public static DateTime NextSlot(string typeName)
        {
            var t = Parameter.TypeOf[typeName];
            return NextSlot(t);
        }

        public static DateTime NextSlot(int type)
        {
            // next will be set to 1:00 even if now is 1:59
            // may want to refine this algorithm either with global or parameter based intervals
            var now = DateTime.Now;
            var diary = Parameter.Types[type].Diary.Split(',').Select(h => int.Parse(h)).ToList();
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
            if (todo.Count == Parameter.Types.Count)
                return todo;

            var planned = todo.Select(t => t.ParameterId);
            foreach (var m in Parameter.Types.Keys.Where(k => !planned.Contains(k)))
            {
                todo.Add(new Sample() { ParameterId = m, NextScheduled = NextSlot(m), LineId = line });
            }
            return todo;
        }

        public virtual void Structure()
        {
            var recs = Type.Count / Reading.CountPerRecord;
            readings = Enumerable.Range(0, recs).Select(i => new Reading(Type)).ToList();
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
            types = Parameter.Types;

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
                    pending = Parameter.Types.Keys.Select(k => new Sample() { ParameterId = k }).ToList();
                    LineId = 0;
                }
                products = AddNone(new SelectList(productlist, "ProductCodeId", "CodeSpec", t.ProductCodeId));
            }
        }

        private SelectList AddNone(SelectList list)
        {
            List<SelectListItem> _list = list.ToList();
            _list.Insert(0, new SelectListItem() { Value = "0", Text = "" });
            return new SelectList(_list, "Value", "Text");
        }
    }
}