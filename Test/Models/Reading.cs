﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using NPoco;
using Omu.ValueInjecter;

namespace Test.Models
{
    public partial class Reading
    {
        public static int CountPerRecord = 5;

        public string FieldName { get; set; }
        public PropertyInfo propInfo { get; set; }
        public static Dictionary<char, int[]> Lines;
        private static string _units = @"ABCD";

        private static string _all = @"
            SELECT m.[ReadingId]
                  ,m.[LineId]
                  ,m.[Stamp]s
                  ,m.[R1]
                  ,m.[R2]
                  ,m.[R3]
                  ,m.[R4]
                  ,m.[R5]
                  ,m.[Average]
                  ,m.[ParameterId]
                  ,m.[Operator]
                  ,m.[ProductCodeId]
                  ,m.[EditCount]
                  ,m.[Scheduled]
                  ,m.[Reel]
                  ,m.[Footage]
                  ,p.[Name]
                  ,p.[Scale]
                  ,p.[Cells]
                  ,p.[Mask]
                  ,p.[Diary]
                  ,p.[Count]
                  ,p.[Icon]
                  ,l.[LineNumber]
                  ,u.[Unit]
                  ,c.[ProductCode]
              FROM [dbo].[Reading] m
              join [Parameter] p on m.ParameterId = p.ParameterId
              join [Line] l on l.LineId = m.LineId
              join [Unit] u on u.UnitId = l.UnitId
              left join [ProductCode] c on m.ProductCodeId = m.ProductCodeId
        ";
        [ResultColumn] public char Unit { get; set; }
        [ResultColumn] public int LineNumber { get; set; }
        [ResultColumn] public string ProductCode { get; set; }

        [ResultColumn, ComplexMapping] public Parameter Prm {get; set;}

        [ResultColumn] public string Name { get; set; }
        [ResultColumn] public string[] Mask { get; set; }
        [ResultColumn] public string[] Cells { get; set; }
        [ResultColumn] public string Icon { get; set; }
        [ResultColumn] public string Diary { get; set; }
        [ResultColumn] public int Count { get; set; }
        [ResultColumn] public bool Passed { get; set; }
        [ResultColumn] public int TestNumber { get; set; }

        public int factorize
        {
            set { factor.Select(f => { f = value; return 1; }).ToList(); }
        }

        private int[] _factor;

        public int[] factor
        {
            get
            {
                if (_factor == null)
                {
                    _factor = Parameter.Types[ParameterId].factor();
                }
                return _factor;
            }
            set { _factor = value; }
        }


        public string r1 { get { return R1.HasValue ? ((double)R1 / factor[0]).ToString() : ""; } }
        public string r2 { get { return R2.HasValue ? ((double)R2 / factor[1]).ToString() : ""; } }
        public string r3 { get { return R3.HasValue ? ((double)R3 / factor[2]).ToString() : ""; } }
        public string r4 { get { return R4.HasValue ? ((double)R4 / factor[3]).ToString() : ""; } }
        public string r5 { get { return R5.HasValue ? ((double)R5 / factor[4]).ToString() : ""; } }

        public string this[int i]
        {
            get
            {
                if (i == 0) return r1;
                if (i == 1) return r2;
                if (i == 2) return r3;
                if (i == 3) return r4;
                if (i == 4) return r5;
                return null;
            }
            set
            {
                double chk = 0.0;
                double.TryParse(value, out chk);
                if (i == 0) R1 = (int) (chk * factor[0]);
                if (i == 1) R2 = (int) (chk * factor[1]);
                if (i == 2) R3 = (int) (chk * factor[2]);
                if (i == 3) R4 = (int) (chk * factor[3]);
                if (i == 4) R5 = (int) (chk * factor[4]);
            }
        }

        private static string _mask = @"'alias': 'decimal', 'digits': {0}, 'integerDigits': {1}";

        public static string inmask(string msk)
        {
            if (msk.Length == 2) return string.Format(_mask, 0, msk[1]);
            return string.Format(_mask, msk[1], msk[2]);
        }

        public string inmsk(int i)
        { 
            var msk = Mask[i];
            return inmask(msk);
        }

        public static void SetLines(IEnumerable<Line> lns)
        {
            Lines = new Dictionary<char, int[]>();
            foreach (var l in lns)
            {
                var unit = _units[l.UnitId-1];
                if (!Lines.ContainsKey(unit))
                    Lines[unit] = new int[8];
                Lines[unit][l.LineNumber - 1] = l.LineId;
            }
            CasingSamplesView.blanks = Reading.Lines.Values.SelectMany((q, i) => q.Select((w, j) => new CasingSample()
                  {
                      Line = new Line() { UnitId = i+1, LineId = w, LineNumber = j+1 },
                      Gly = new Reading(0, "CasingSample")
                  }));
        }

        public static List<Reading> All()
        {
            using (labDB t = new labDB())
            {
                var r = t.Fetch<Reading>(_all + " order by [Scheduled] desc");
                return r;
            }
        }

        static Reading()
        {
            SetLines(repo.Fetch<Line>());
        }

        public Reading() { }
        
        public Reading(Parameter p)
        {
            ParameterId = p.ParameterId;
            parameter();
        }

        public Reading(int sampleid, int parameterid)
        {
            Reading nr = null;
            using (labDB d = new labDB())
            {
                nr = d.FirstOrDefaultInto(nr, " where sampleid = @0", sampleid);
            }
            ParameterId = parameterid;
            parameter();
        }

        public Reading(int id, string name) : this(id, Parameter.TypeOf[name])
        {}

        public Reading(int id, int? type)
        {
            if (id == 0)
                ParameterId = type.Value;
            else
            {
                Reading nr = null;
                using (labDB d = new labDB())
                {
                    nr = d.SingleById<Reading>(id);
                    this.InjectFrom(nr);
                }
            }
            parameter();
        }

        public int parameter()
        {
            var t = Parameter.Types[ParameterId];
            Name = t.Name;
            factor = t.factor();
            Mask = t.Mask.Split(',');
            Count = t.Count;
            Icon = t.Icon;
            return Count;
        }
    }

    public class ReadItem
    {
        public string CellClass { get; set; }
        public string Mask { get; set; }
        public string Value { get; set; }

        public ReadItem() { }

        public ReadItem(string mask)
        {
            Value = "";
            Mask = mask;
        }
        public ReadItem(string value, string mask)
        {
            Value = value;
            Mask = mask;
        }
    }

    public class ReadingView
    {
        public Sample s { get; set; }
        public Reading r { get; set; }

        public DateTime nextslot {
            get
            {
                var now = DateTime.Now;
                return new DateTime(now.Year, now.Month, now.Day, Parameter.Times[r.ParameterId].First(t => t >= now.Hour), 0, 0);
            } 
        }
        
        public Dictionary<char, int[]> grid { get; set; }

        public ReadingView() { }

        //public ReadingView(int id, int? type)
        //{
        //    grid = Reading.Lines;
        //    r = new Reading(id, type);
        //    s = new Sample(r.SampleId);
        //}

        public ReadingView(int id, Sample s)
        {
            grid = Reading.Lines;
            var type = s == null ? null : s.ParameterId;
            r = new Reading(id, type);
            r.SampleId = s.SampleId;
        }
    }
}