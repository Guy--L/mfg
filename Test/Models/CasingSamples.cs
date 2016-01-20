using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Omu.ValueInjecter;
using System.IO;
using System.Web;
using System.Diagnostics;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System.Web.Mvc;
using NPoco;

namespace Test.Models
{
    public class CasingSample : Sample
    {
        public static Dictionary<string, PropertyInfo> reflect;
        private static string casingType = "CasingSample";

        private static int _type = Reading.TypeOf[casingType];
        private static int oiltype = Reading.TypeOf["Oil"];

        public static string Symbol = Reading.IconML(_type);

        public Reading Gly { get; set; }
        public Reading Oil { get; set; }

        public int? Delm { get { return Reading1; } set { Reading1 = value; } }
        public int? Roto { get { return Reading2; } set { Reading2 = value; } }
        public int? OilPct { get { return Reading3; } set { Reading3 = value; } }
        public string oilPct { get { return Reading3.HasValue ? ((double)Reading3 / 10.0).ToString() : ""; } set { Reading3 = int.Parse(value); } }

        public int? Moist { get { return Gly?.R1; } set { Gly.R1 = value; } }
        public int? GlyWt { get { return Gly?.R2; } set { Gly.R2 = value; } }
        public int? DryWt { get { return Gly?.R3; } set { Gly.R3 = value; } }
        public int? GlyArea {
            get { return Gly?.R4; }
            set { Gly.R4 = value; }
        }
        public int? GlySTD { get { return Gly?.R5; } set { Gly.R5 = value; } }

        public double[] _moist;
        public double[] _glyc;
        public string _moistspec;
        public string _glyspec;

        public string Tooltip { get; set; }
        public string System { get; set; }
        public string StatIcon { get; set; }
        public string StatColor { get; set; }
        public string LineName { get; set; }

        public bool isPublished { get { return !((Completed ?? DateTime.MaxValue) > DateTime.Now); } }
        public ProductCode _product; 
        public ProductCode product
        {
            get { return _product; }
            set
            {
                _product = value;
                MoistSpec = _product.MoistSpec;
                GlySpec = _product.GlySpec;
            }
        }

        public string MoistSpec {
            get
            {
                if (product == null) return "0<50<100";
                return product.MoistSpec;
            }
            set
            {
                var newm = value;
                if (_moist == null || newm != _moistspec)
                {
                    _moist = newm.Split('<').Select(i => string.IsNullOrWhiteSpace(i) ? 0.0 : double.Parse(i)).ToArray();
                    _moistspec = newm;
                }
            }
        }
        public string GlySpec {
            get
            {
                if (product == null) return "0<50<100";
                return product.GlySpec;
            }
            set
            {
                var newg = value;
                if (_glyc == null || newg != _glyspec)
                {
                    _glyc = newg.Split('<').Select(i => string.IsNullOrWhiteSpace(i) ? 0.0 : double.Parse(i)).ToArray();
                    _glyspec = newg;
                }
            }
        }
        public string MoistStatus
        {
            get
            {
                string speclass = "";
                if (OutOfControl(MoistPct, _moist, -5)) speclass = "oocontrol";
                if (OutOfSpec(MoistPct, _moist)) speclass = "oospec";
                Debug.Write("line " + LineName + " moisture: " + speclass + ". "+MoistPct + " in ");
                foreach (var d in _moist)
                {
                    Debug.Write(d + " < ");
                }
                Debug.WriteLine(".");
                return speclass;
            }
        }
        public string GlyStatus
        {
            get
            {
                string speclass = "";
                if (OutOfControl(GlyPct, _glyc, 10)) speclass = "oocontrol";
                if (OutOfSpec(GlyPct, _glyc)) speclass = "oospec";
                return speclass;
            }
        }
        public string SpecStatus {
            get
            {
                string speclass = "";
                if (OutOfControl(MoistPct, _moist, -5) || OutOfControl(GlyPct, _glyc, 10))
                    speclass = "oocontrol";
                if (OutOfSpec(MoistPct, _moist) || OutOfSpec(GlyPct, _glyc))
                    speclass = "oospec";

                return speclass;
            }
        }

        public double? MoistPct {
            get {
                if (!DryWt.HasValue || !Moist.HasValue || Moist.Value == 0.0 || DryWt.Value == 0.0)
                    return null;
                return (1 - ((double)DryWt / (double)Moist)) * 100.0;
                // var moistpct = (1 - r3 / r1) * 100;
            }
        }

        public bool OutOfSpec(double? actual, double[] spec)
        {
            if (!actual.HasValue || spec == null) return false;
            var spc = spec.Select(s => Math.Round(s, 1)).ToArray();
            var val = Math.Round(actual.Value, 1);
            //Debug.WriteLine("round: " + spc[0] + " < " + val + " < " + spc[2] + ", not: " + spec[0] + " < " + actual.Value + " < " + spec[2]);
            return val < spc[0] || spc[2] < val;
        }

        public bool OutOfControl(double? actual, double[] spec, int margin)
        {
            if (!actual.HasValue || spec == null) return false;
            double low, high;
            if (margin < 0) {
                low = spec[0];
                high = spec[2];
            }
            else { low = high = spec[1]; }
            var val = Math.Round(actual.Value, 1);
            low -= ((double)margin / 10.0);
            high += ((double)margin / 10.0);
            var result =  (val < low || high < val);
            Debug.WriteLine("actual: " + actual.Value + ", round: "+ val+", low: " + low + ", high: " + high+", result: "+result);
            return result;
        }

        public double? GlyPct {
            get {
                if (MoistPct == null) return null;
                if (!GlyArea.HasValue || !GlySTD.HasValue || !GlyWt.HasValue || GlyArea.Value == 0 || GlySTD.Value == 0 || GlyWt.Value == 0)
                    return null;
                  
                return ((double)GlyArea/(double)GlySTD/2.0 / ((double)DryWt / (double)Moist * (double)GlyWt/1000.0 * (1 - (double) (Reading3??0)/ 1000.0))  )*100.0;
                // var glypct = (r4/r5/2 / ( r3 / r1 * r2 / 1000 * (1 - OilPct / 100) * 100;
            }
        }

        public HtmlString InputMask(string msk)
        {
            return new HtmlString(Reading.inmask(msk));
        }

        public HtmlString SampledProduct { get; set; }

        public static void mapReflection()
        {
            var c = new CasingSample();
            reflect = c.GetType().GetProperties().Select(p => new { p.Name, p }).ToDictionary(r => r.Name, r => r.p);
            reflect.Add("Gly_r1", reflect["Moist"]);
            reflect.Add("Gly_r2", reflect["GlyWt"]);
            reflect.Add("Gly_r3", reflect["DryWt"]);
            reflect.Add("Gly_r4", reflect["GlyArea"]);
            reflect.Add("Gly_r5", reflect["GlySTD"]);
            reflect["oilPct"] = reflect["OilPct"];
        }

        public static DateTime NextSlot()
        {
            return NextSlot(_type);
        }

        public CasingSample Apply(Reading r)
        {
            var s = new Reading(0, _type);
            s.SampleId = SampleId;
            if (r != null)
            {
                s.ReadingId = r.ReadingId;
                s.R1 = r.R1;
                s.R2 = r.R2;
                s.R3 = r.R3;
                s.R4 = r.R4;
                s.R5 = r.R5;
            }

            //if (ParameterId == _type)
                Gly = s;
            //else
            //    Oil = s;

            return this;
        }

        public CasingSample() { }

        public CasingSample(Line ln)
        {
            product = ln.product;
            Gly = new Reading(0, _type);
            StatColor = ln.status.Color;
            StatIcon = ln.status.Icon;
            LineId = ln.LineId;
            UnitId = ln.UnitId;
            LineName = ln.Name;
        }

        public CasingSample(int id)
        {
            if (id != 0)
            {
                var s = new Sample();
                using (labDB d = new labDB())
                {
                    s = d.SingleById<Sample>(id);
                }
                this.InjectFrom(s);
                return;
            }
            Tech = "";
            Note = "";
            ProductCodeId = 0;
            Stamp = DateTime.Now;
            ParameterId = _type;
        }

        public void Select()
        {
            var time = Scheduled.ToString("yyyy-MM-dd HH:mm:ss");

            var s = new Sample();
            using (labDB d = new labDB())
            {
                s = d.SingleOrDefault<Sample>(" where scheduled = @0 and lineid = @1", time, LineId);
            }
            if (s != null) 
                this.InjectFrom(s);
            return;
        }

        public CasingSample(IRow r, DateTime sampled, string tech) : base(0)
        {
            Stamp = sampled;
            Scheduled = sampled;
            Note = "";
            Tech = tech.Replace(".","").Trim().Substring(0, 2);
            var linecell = r.GetCell(0);
            if (linecell?.CellType != CellType.Numeric)
            {
                LineId = 0;
                return;
            }
            var line = (int)linecell.NumericCellValue;
            if (!Unit.lineids.ContainsKey(line))
            {
                LineId = 0;
                return;
            }
            LineId = Unit.lineids[(int)line];

            Select(); 

            var meter = r.GetCell(1).StringCellValue.Replace("--","-").Split('-');

            int reel = 0;
            int.TryParse(meter[0], out reel);
            ReelNumber = reel;

            reel = 0;
            if (meter.Length > 1)
            {
                var ft = meter[1].Replace(".", "").Replace(",", "");
                int.TryParse(ft, out reel);
            }
            Footage = reel;

            var delmcell = r.GetCell(2);
            if (delmcell?.CellType != CellType.Numeric)
            {
                LineId = 0;
                return;
            }
            try
            {
                Delm = (int)delmcell.NumericCellValue;
                var rotocell = r.GetCell(13);
                Roto = (rotocell.CellType == CellType.Numeric)? (int)rotocell.NumericCellValue: 0;                    // sometimes a text cell
                var oilcell = r.GetCell(12);
                OilPct = (oilcell?.CellType == CellType.Numeric) ? (int) (oilcell.NumericCellValue * 1000.0) : 0;

                Save();
            }
            catch(Exception e)
            {
                Debug.WriteLine("saving CasingSample " + CasingSamples.current + ", " + LineId + ": " + e.Message+"\n"+e.StackTrace);
            }

            try
            {   // replace if already in database

                Gly = new Reading(SampleId, _type);
                Gly.SampleId = SampleId;
                Moist = (int)(r.GetCell(3).NumericCellValue * Gly.factor[0]);
                GlyWt = (int)(r.GetCell(6).NumericCellValue * Gly.factor[1]);
                DryWt = (int)(r.GetCell(4).NumericCellValue * Gly.factor[2]);
                GlyArea = (int)r.GetCell(8).NumericCellValue;
                GlySTD = (int)r.GetCell(9).NumericCellValue;
                Gly.Scheduled = sampled;
                Gly.Stamp = sampled;
                Gly.Operator = tech;
                Gly.EditCount = 1;
                Gly.Save();
            }
            catch (Exception e)
            {
                Debug.WriteLine("saving Gly " + Stamp + ", " + LineId + ": " + e.Message);
            }
        }

        public void ToRow(IRow r)
        {
            r.GetCell(1).SetCellValue(ReelNumber + "-" + Footage);
            try {
                if (Delm.HasValue) r.GetCell(2).SetCellValue(Delm.Value);
                if (Roto.HasValue) r.GetCell(13).SetCellValue(Roto.Value);
                if (OilPct.HasValue) r.GetCell(12).SetCellValue((double)OilPct.Value / (double)1000.0);
                if (Moist.HasValue) r.GetCell(3).SetCellValue((double)Moist.Value / (double)Gly.factor[0]);
                if (GlyWt.HasValue) r.GetCell(6).SetCellValue((double)GlyWt.Value / (double)Gly.factor[1]);
                if (DryWt.HasValue) r.GetCell(4).SetCellValue((double)DryWt.Value / (double)Gly.factor[2]);
                if (GlyArea.HasValue) r.GetCell(8).SetCellValue(GlyArea.Value);
                if (GlySTD.HasValue) r.GetCell(9).SetCellValue(GlySTD.Value);
                r.GetCell(10).CellStyle.FillPattern = FillPattern.NoFill;
                r.GetCell(5).CellStyle.FillPattern = FillPattern.NoFill;
                
                if (GlyStatus.Contains("control"))     r.GetCell(10).CellStyle = CasingSamplesView.yellow;
                else if (GlyStatus.Contains("spec"))   r.GetCell(10).CellStyle = CasingSamplesView.red;

                if (MoistStatus.Contains("control"))   r.GetCell(5).CellStyle = CasingSamplesView.yellow;
                else if (MoistStatus.Contains("spec")) r.GetCell(5).CellStyle = CasingSamplesView.red;
            }
            catch(Exception e) { Debug.WriteLine(Line + "error: " + e.Message + "\nstack: "+e.StackTrace); }
        }
    }

    public class CasingSurvey
    {
        public DateTime scheduled { get; set; }
        public string julianshift
        {
            get
            {
                var d = scheduled.AddHours(6);
                return d.DayOfYear + " " + ((d.Hour - 1) / 6 + 1);
            }
        }
        public int id { get; set; }
        public int count { get; set; }
        public int registered { get; set; }
        public int wet { get; set; }
        public int dry { get; set; }
        public int gly { get; set; }
        public bool complete { get; set; }

        public CasingSurvey() { }
    }

    public class CasingSamples
    {
        private static string _all = @";
            with rdg as (
                select r.sampleid 
                    ,sum(case when r.r1 is null and r.r2 is null then 0 else 1 end) as wet
                    ,sum(case when r.r3 is null then 0 else 1 end) as dry
                    ,sum(case when r.r4 is null and r.r5 is null then 0 else 1 end) as gly
                from reading r 
                join parameter p on p.parameterid = r.parameterid
                where p.name = 'CasingSample'
                group by r.sampleid
            )
            select s.scheduled
                ,min(s.sampleid) as id
                ,count(s.sampleid) as count
                ,sum(case when s.reelnumber is null or s.footage is null then 0 else 1 end) as registered 
                ,sum(q.wet) as wet
                ,sum(q.dry) as dry
                ,sum(q.gly) as gly
                ,case when sum(case when s.completed is null then 0 else 1 end) = count(s.sampleid) then 1 else 0 end as complete
            from sample s
            left join rdg q on s.sampleid = q.sampleid
            group by s.scheduled
            order by s.scheduled desc
        ";
        public List<CasingSurvey> batches { get; set; }

        public CasingSamples()
        {
            using (labDB d = new labDB())
            {
                d.OneTimeCommandTimeout = 120;
                batches = d.Fetch<CasingSurvey>(_all);
            }
        }

        private static int[] _actual = new int[] { -5, 1, 7, 13 };
        //public static string ReadExcels(string path, int year)
        //{
        //    int filecount = 0;
        //    int samplecount = 0;

        //    var files = Directory.GetFiles(path, "*.xlsx", SearchOption.TopDirectoryOnly);
        //    DateTime batch = new DateTime(year, 1, 1, 0, 0, 0);

        //    foreach (var set in files)
        //    {
        //        var w = new XLWorkbook(set);
        //        IXLWorksheet s = null;
        //        try
        //        {
        //            s = w.Worksheet("Gly & Moist");
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.WriteLine("exception: " + e.Message);
        //            continue;
        //        }
        //        var julian = s.Cell("C3").GetValue<int>();
        //        var shift = s.Cell("E3").GetValue<int>();
        //        var tech = s.Cell("H3").GetValue<string>();

        //        var stamp = batch.AddDays(julian).AddHours(_actual[shift - 1]);
        //        int row = 7;

        //        while (row++ < 56)
        //        {
        //            var r = s.Row(row);
        //            if (r.IsEmpty() || r.Cell(1).IsEmpty() || r.Cell(2).IsEmpty())
        //                continue;
        //            new CasingSample(r, stamp, tech);
        //            samplecount++;
        //        }
        //        filecount++;
        //    }
        //    return filecount + " files read, " + samplecount + " samples recorded";
        //}
        public static Tuple<int?, DateTime?> ReadExcel(Stream file)
        {
            var now = DateTime.Now;
            int year = now.Year;
            int day = now.DayOfYear;

            int samplecount = 0;
            DateTime stamp = DateTime.MinValue;

            HSSFWorkbook hssfwb = null;
            try {
                hssfwb = new HSSFWorkbook(file);
            }
            catch (Exception e)
            {
                return new Tuple<int?, DateTime?>(0, DateTime.MinValue);
            }
            string linerpt = "";
            try
            {
                ISheet s = hssfwb.GetSheet("Gly & Moist");
                var julian = s.GetRow(2).GetCell(2)?.NumericCellValue;
                var shiftcell = s.GetRow(2).GetCell(4);
                if (shiftcell?.CellType != CellType.Numeric)
                    return null;
                var shift = (int)shiftcell.NumericCellValue;
                var tech = s.GetRow(2).GetCell(7)?.StringCellValue;

                if (shift < 1 || shift > 4)
                {
                    Debug.WriteLine("(" + linerpt + ") weird shift in ReadExcel: " + shift);
                    return null;
                }
                if (day < julian - 200)         // if loading a sheet from the end of december
                {
                    year--;
                }
                DateTime batch = new DateTime(year, 1, 1, 0, 0, 0);
                stamp = batch.AddDays((int)julian - 1).AddHours(_actual[shift - 1]);

                for (int row = 7; row <= s.LastRowNum && row < 56; row++)
                {
                    if (s.GetRow(row) == null || s.GetRow(row).GetCell(0) == null || s.GetRow(row).GetCell(1) == null) //null is when the row only contains empty cells 
                        continue;
                    linerpt = "row: " + row;
                    var c = new CasingSample(s.GetRow(row), stamp, tech);
                    linerpt = "line: " + c.LineId + ", row: " + row;
                    if (c.LineId == 0)
                        continue;
                    samplecount++;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("(" + current + "," + linerpt + ") exception in ReadExcel: " + e.Message);
                Debug.WriteLine(e.StackTrace);
                return new Tuple<int?, DateTime?>(0, DateTime.MinValue);
            }
            return new Tuple<int?, DateTime?>(samplecount, stamp);
        }

        public static string current = "";
        public static string ReadExcels(string path, int year)
        {
            int filecount = 0;
            int samplecount = 0;

            var files = Directory.GetFiles(path, "*.xls", SearchOption.TopDirectoryOnly);

            foreach (var set in files)
            {
                using (FileStream file = new FileStream(set, FileMode.Open, FileAccess.Read))
                {
                    current = set;
                    var count = ReadExcel(file);
                    samplecount += count.Item1.HasValue ? count.Item1.Value : 0;
                    if (count.Item1.HasValue) filecount++;
                }
            }
            return filecount + " files read, " + samplecount + " samples recorded";
        }

        public static string ReadExcelX(string path, int year)
        {
            int filecount = 0;
            int samplecount = 0;
            HashSet<string> hash = new HashSet<string>();

            var files = Directory.GetFiles(path, "*.xls", SearchOption.TopDirectoryOnly);
            DateTime batch = new DateTime(year, 1, 1, 0, 0, 0);

            foreach (var set in files)
            {
                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(set, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }
                string linerpt = "";
                try
                {
                    ISheet s = hssfwb.GetSheet("Gly & Moist");
                    var julian = s.GetRow(2).GetCell(2)?.NumericCellValue;
                    var shiftcell = s.GetRow(2).GetCell(4);
                    if (shiftcell?.CellType != CellType.Numeric)
                        continue;
                    var shift = (int)shiftcell.NumericCellValue;
                    var tech = s.GetRow(2).GetCell(7)?.StringCellValue;

                    if (shift < 1 || shift > 4)
                    {
                        Debug.WriteLine("(" + set + ", " + linerpt + ") weird shift in ReadExcel: " + shift);
                        continue;
                    }

                    var stamp = batch.AddDays((int)julian).AddHours(_actual[shift - 1]);

                    for (int row = 7; row <= s.LastRowNum || row < 56; row++)
                    {
                        if (s.GetRow(row) == null || s.GetRow(row).GetCell(0) == null || s.GetRow(row).GetCell(1) == null) //null is when the row only contains empty cells 
                            continue;
                        linerpt = "row: " + row;
                        var r = s.GetRow(row);
                        var defacto = r.GetCell(1).StringCellValue.Trim();
                        var meter = defacto.Replace("--","-").Split('-');

                        int reel = 0;
                        int.TryParse(meter[0], out reel);

                        reel = 0;
                        if (meter.Length > 1 && int.TryParse(meter[1].Replace(".","").Replace(",",""), out reel))
                            continue;

                        hash.Add(defacto);
                    }
                    filecount++;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("(" + set + ", " + linerpt + ") exception in ReadExcel: " + e.Message);
                }
            }
            var uniques = filecount + " files read, " + samplecount + " samples recorded\n" + string.Join("<br />", hash);
            return uniques;
        }
    }

    public class CasingSamplesView
    {
        public string Symbol { get { return CasingSample.Symbol; } }

        private static string _setproduct = @"
            update [sample] set productcodeid = {0} where sampleid = {1}
        ";

        private static string _batch = @"
            SELECT s.[SampleId]
                  ,s.[Scheduled]
                  ,s.[Stamp]
                  ,s.[LineId]
                  ,s.[Note]
                  ,s.[Tech]
                  ,s.[Completed]
                  ,s.[ReelNumber]
                  ,s.[Footage]
                  ,s.[BarCode]
                  ,s.[ParameterId]
                  ,s.[Reading1]
                  ,s.[Reading2]
                  ,s.[Reading3]
                  ,s.[ProcessId]
                  ,s.[SystemId]
                  ,s.[NextScheduled]
                  ,l.[UnitId]
				  ,l.[LineNumber]
                  ,x.[StatusId]
                  ,x.[Code]
                  ,x.[Description]
                  ,x.[Icon]
                  ,x.[Color]
                  ,r.[ReadingId]
                  ,r.[LineId]
                  ,r.[Stamp]
                  ,r.[R1]
                  ,r.[R2]
                  ,r.[R3]
                  ,r.[R4]
                  ,r.[R5]
                  ,r.[Average]
                  ,r.[ParameterId]
                  ,r.[Operator]
                  ,r.[EditCount]
                  ,r.[Scheduled]
                  ,p.[ProductCodeId]
                  ,p.[ProductCode]
                  ,p.[ProductSpec]
                  ,p.[ReelMoist_Min]
                  ,p.[ReelMoist_Aim]
                  ,p.[ReelMoist_Max]
                  ,p.[Gly_Min]
                  ,p.[Gly_Aim]
                  ,p.[Gly_Max]
                  ,p.[Oil_Min]
                  ,p.[Oil_Aim]
                  ,p.[Oil_Max]
              FROM [dbo].[Sample] s
              left join [dbo].[Reading] r on r.SampleId = s.SampleId
              left join [Line] l on l.LineId = s.LineId
              left join [Status] x on l.StatusId = x.StatusId
              left join [ProductCode] p on p.ProductCodeId = s.ProductCodeId
        ";

        private static string _bysample = @"
            where     s.Scheduled = (select Scheduled from Sample where SampleId = @0)
            order by s.LineId
        ";

        private static string _bytime = @"
            where s.Scheduled = @0
            order by s.LineId
        ";

        private static string _tech;

        public string Tech
        {
            get { return _tech; }
            set { _tech = value; }
        }

        public DateTime ScheduleTime { get; set; }
        public int Julian { get { return ScheduleTime.AddHours(6).DayOfYear; } }
        public int Shift { get { return ((ScheduleTime.AddHours(6).Hour - 1) / 6) + 1; } }
        public string JulianShift
        {
            get
            {
                var d = ScheduleTime.AddHours(6);
                var julian = d.DayOfYear;
                var shift = ((d.Hour - 1) / 6) + 1;
                return julian + " " + shift;
            }
        }
        public ILookup<int, CasingSample> samples
        {
            get; set;
        }
        public bool allCompleted = false;
        public SelectList products;
        public int productCode;

        public static IEnumerable<CasingSample> blanks;

        private static CasingSample peg = null;
        private static CasingSample Link(Sample s, Status x, Reading r, ProductCode p)
        {
            if (peg == null || peg.SampleId != s.SampleId)
            {
                peg = new CasingSample();
                peg.InjectFrom(s);
                if (!string.IsNullOrWhiteSpace(s.Tech))
                    _tech = s.Tech;
            }
            if (p == null)
                p = new ProductCode() { ProductCodeId = 0, _ProductCode = "00?00" };
            peg.product = p;
            peg.LineName = s.Line;
            peg.StatColor = x.Color;
            peg.StatIcon = x.Icon;

            return peg.Apply(r);
        }

        public static ICellStyle red;
        public static ICellStyle yellow;

        public string Export(string template)
        {
            var path = Path.Combine(Path.GetTempPath(), "x" + JulianShift.Replace(' ', '-') + ".xls");
            Debug.WriteLine("Target: " + path);
            HSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(template, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
                int row = 7;

                red = hssfwb.CreateCellStyle();
                yellow = hssfwb.CreateCellStyle();

                try
                {
                    ISheet s = hssfwb.GetSheet("Gly & Moist");

                    s.GetRow(2).GetCell(2).SetCellValue(Julian);
                    s.GetRow(2).GetCell(4).SetCellValue(Shift);

                    var pctstyle = s.GetRow(7).GetCell(5).CellStyle;

                    red.CloneStyleFrom(pctstyle);
                    red.FillForegroundColor = HSSFColor.Red.Index;
                    red.FillPattern = FillPattern.SolidForeground;

                    yellow.CloneStyleFrom(pctstyle);
                    yellow.FillForegroundColor = HSSFColor.Yellow.Index;
                    yellow.FillPattern = FillPattern.SolidForeground;

                    var casings = samples.SelectMany(x => x.Select(y => y)).ToList();

                    int lastempty = 0;
                    Dictionary<int, List<CasingSample>> retests = new Dictionary<int, List<CasingSample>>();
                    IRow r = null;
                    string tech = "";
                    while (row < s.LastRowNum + 1)
                    {
                        r = s.GetRow(row++);
                        var c = r.GetCell(0);
                        if (c == null || c.CellType == CellType.Blank || c.CellType != CellType.Numeric)
                            continue;

                        var lineid = Unit.lineids[(int)c.NumericCellValue];
                        var match = casings.Where(q => q.LineId == lineid);
                        if (match == null || match.Count() == 0)
                        {
                            lastempty = row - 1;
                            continue;
                        }

                        match.First().ToRow(r);
                        tech = match.First().Tech;
                        retests.Add((int)c.NumericCellValue, match.Skip(1).ToList());
                    }
                    s.GetRow(2).GetCell(7).SetCellValue(tech);
                    row = lastempty;
                    foreach (var d in retests)
                    {
                        foreach (var cs in d.Value)
                        {
                            r = s.GetRow(row++);
                            r.GetCell(0).SetCellValue(d.Key);
                            cs.ToRow(r);
                        }
                    }
                    s.ForceFormulaRecalculation = true;

                    var f = new FileStream(path, FileMode.Create);
                    hssfwb.Write(f);
                    f.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("WriteExcel, row " + row + ": " + e.Message + "\n" + e.StackTrace);
                }
                file.Close();
            }

            return path;
        }

        /// <summary>
        /// Merge data from product, line, sample into one UI.
        /// </summary>
        /// <param name="id">Id of one of the samples in the collection used to retrieve collection</param>
        public CasingSamplesView(int id)
        {
            allCompleted = false;
            IEnumerable<CasingSample> sset = null;
            List<CasingSample> complete = new List<CasingSample>();
            var lines = new Lines();

            using (labDB d = new labDB())
            {
                products = new SelectList(d.Fetch<ProductCode>(" order by productcode, productspec"), "ProductCodeId", "CodeSpec");
                if (id == 0)
                {
                    ScheduleTime = CasingSample.NextSlot();
                    var time = ScheduleTime.ToString("yyyy-MM-dd HH:mm:ss");
                    sset = d.Fetch<Sample, Status, Reading, ProductCode, CasingSample>(Link, _batch + _bytime, time);
                }
                else
                {
                    sset = d.Fetch<Sample, Status, Reading, ProductCode, CasingSample>(Link, _batch + _bysample, id);
                    ScheduleTime = sset.First().Scheduled;
                }
            }
            allCompleted = sset != null && sset.Count() > 0 && !sset.Any(s => (s.Completed ?? DateTime.MaxValue) > DateTime.Now);

            if (allCompleted)
            {
                samples = sset.ToLookup(k => k.UnitId, v => v);
                return;
            }

            var noProduct = sset.Where(s => s.ProductCodeId == 0).ToList();             // not lazy evaluation

            foreach (var line in lines.lines)
            {
                var add = sset?.Where(s => s.LineId == line.LineId).Select(s => s);
                if (add?.Count() > 0)
                {
                    add.Any(n =>
                    {
                        n.ProductCodeId = line.ProductCodeId;
                        n.product = line.product;
                        n.MoistSpec = line.product.MoistSpec;
                        n.GlySpec = line.product.GlySpec;
                        return false;
                    });
                    complete.AddRange(add);
                    continue;
                }
                complete.Add(new CasingSample(line));
            }

            if (noProduct.Any())
            {
                var setproduct = string.Join("", noProduct.Select(p => string.Format(_setproduct, p.ProductCodeId, p.SampleId)).ToArray());
                using (labDB d = new labDB())
                {
                    d.Execute(setproduct);
                }
            }

            samples = complete.ToLookup(k => k.UnitId, v => v);
        }

        private static string _complete = @"
            update [sample] set completed = getdate() where sampleid in ({0})
        ";

        // move results from lab database to tags database
        // calculations are performed during the move 
        // 
        private static string _labresult = @"
            declare @@insertedtags table
            (
	            tagid int, 
	            sampleid int
            )

            merge into [All] as target
            using (
	            select l.sampleid, r.tagid,
	            cast(round((1 - l.r3 / l.r1) * 100.0, 1) as varchar(64)) as value,
	            l.stamp,
	            192 as quality
	            from mesdb.dbo.[LabResult] l
	            join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	            join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	            where f.FieldName = 'csg_moist_pct' 
		            and l.Completed is not null 
		            and l.sampleid in ({0})	
	            ) as source (sampleid, tagid, value, stamp, quality)
            on 1 = 0
            when not matched then
	            insert (tagid, value, stamp, quality)
	            values (tagid, value, stamp, quality)
            output inserted.tagid, source.sampleid into @@insertedtags;

            merge into [All] as target
            using (
	            select l.sampleid, r.tagid,
                cast(round((l.r4 / l.r5 / 2.0 / ( l.r3 / l.r1 * l.r2 / 1000.0 * (1 - l.OilPct / 1000.0 ))) * 100.0, 1) as varchar(64)) as value,
	            l.stamp,
	            192 as quality
	            from mesdb.dbo.[LabResult] l
	            join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	            join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	            where f.FieldName = 'csg_glyc_pct' 
		            and l.Completed is not null 
		            and l.sampleid in ({0})	
	            ) as source (sampleid, tagid, value, stamp, quality)
            on 1 = 0
            when not matched then
	            insert (tagid, value, stamp, quality)
	            values (tagid, value, stamp, quality)
            output inserted.tagid, source.sampleid into @@insertedtags;

            update t 
            set t.relatedtagid = i.sampleid 
            from tag t
            join @@insertedtags i on t.tagid = i.tagid
        ";        

        public void Seal()
        {
            
            var ids = string.Join(",",samples.SelectMany(x => x.Select(y => y.SampleId)).Where(k => k != 0).Distinct().ToArray());
            if (ids.Length == 0)
                return;

            using (labDB d = new labDB())
            {
                d.Execute(string.Format(_complete, ids));
            }
            using (tagDB t = new tagDB())
            {
                t.Execute(string.Format(_labresult, ids));
            }
        }
    }
}