using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [ResultColumn, ComplexMapping] public Reading Gly { get; set; }

        public Reading Oil { get; set; }

        public int? Delm { get { return Reading1; } set { Reading1 = value; } }
        public int? Roto { get { return Reading2; } set { Reading2 = value; } }
        public int? OilPct { get { return Reading3; } set { Reading3 = value; } }
        public string oilPct { get { return Reading3.HasValue ? ((double)Reading3 / 10.0).ToString() : ""; } set { Reading3 = int.Parse(value); } }

        public int? Moist { get { return Gly?.R1; } set { Gly.R1 = value; } }
        public int? GlyWt { get { return Gly?.R2; } set { Gly.R2 = value; } }
        public int? DryWt { get { return Gly?.R3; } set { Gly.R3 = value; } }
        public int? GlyArea
        {
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

        public bool isPublished { get { return !((Completed ?? DateTime.MaxValue) > DateTime.Now); } }

        public bool _isUpdating; 

        [ResultColumn, ComplexMapping] public ProductCode _product;
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

        public string MoistSpec
        {
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
        public string GlySpec
        {
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
                //Debug.Write("line " + LineName + " moisture: " + speclass + ". " + MoistPct + " in ");
                //foreach (var d in _moist)
                //{
                //    Debug.Write(d + " < ");
                //}
                //Debug.WriteLine(".");
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
        public string SpecStatus
        {
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

        public double? MoistPct
        {
            get
            {
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
            if (margin < 0)
            {
                low = spec[0];
                high = spec[2];
            }
            else { low = high = spec[1]; }
            var val = Math.Round(actual.Value, 1);
            low -= ((double)margin / 10.0);
            high += ((double)margin / 10.0);
            var result = (val < low || high < val);
            Debug.WriteLine("actual: " + actual.Value + ", round: " + val + ", low: " + low + ", high: " + high + ", result: " + result);
            return result;
        }

        public double? GlyPct
        {
            get
            {
                if (MoistPct == null) return null;
                if (!GlyArea.HasValue || !GlySTD.HasValue || !GlyWt.HasValue || GlyArea.Value == 0 || GlySTD.Value == 0 || GlyWt.Value == 0)
                    return null;

                return ((double)GlyArea / (double)GlySTD / 2.0 / ((double)DryWt / (double)Moist * (double)GlyWt / 1000.0 * (1 - (double)(Reading3 ?? 0) / 1000.0))) * 100.0;
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

        public CasingSample() {
            Gly = new Reading(0, _type);
        }

        public CasingSample(LineTx ln)
        {
            product = ln.product;
            Gly = new Reading(0, _type);
            StatColor = ln.status.Color;
            StatIcon = ln.status.Icon;
            Line = new Line()
            {
                LineId = ln.LineId,
                UnitId = ln.UnitId
            };
        }

        public CasingSample(Line ln)
        {
            product = ln.product;
            Gly = new Reading(0, _type);
            StatColor = ln.status.Color;
            StatIcon = ln.status.Icon;
            Line = new Line()
            {
                LineId = ln.LineId,
                UnitId = ln.UnitId
            };
        }

        private string _previous = @";
            declare @@sid int

            select @@sid = @0

            select top 1 @@sid = s.sampleid 
            from [sample] s
            left join [sample] t 
            on t.productcodeid = s.productcodeid 
            and t.lineid = s.lineid
            and t.stamp > s.stamp
            where t.sampleid = @0
            order by s.stamp desc
        ";

        private string _next = @";
            declare @@sid int

            select @@sid = @0

            select top 1 @@sid = s.sampleid 
            from [sample] s
            left join [sample] t 
            on t.productcodeid = s.productcodeid 
            and t.lineid = s.lineid
            and t.stamp < s.stamp
            where t.sampleid = @0
            order by s.stamp
        ";

        public string LotNum
        {
            get
            {
                return "B" + (Stamp.Year % 10).ToString() + Stamp.DayOfYear.ToString("000") + Line.Name + ReelNumber.ToString("000");
            }
        }

        public CasingSample(int id, int direction)
        {
            if (id != 0)
            {
                using (labDB d = new labDB())
                {
                    try
                    {
                        if (direction == 0)
                            d.SingleInto(this, CasingSamplesView._batch + " where s.sampleid = @0", id);
                        else if (direction > 0)
                            d.SingleInto(this, _next + CasingSamplesView._batch + " where s.sampleid = @@sid", id);
                        else
                            d.SingleInto(this, _previous + CasingSamplesView._batch + " where s.sampleid = @@sid", id);
                    }
                    catch (Exception e)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new NotImplementedException("Error in CasingSample() " + e.Message));
                    }
                }
                return;
            }
            Tech = "";
            Note = "";
            ProductCodeId = 0;
            Stamp = DateTime.Now;
            ParameterId = _type;
        }

        public CasingSample(int id)
        {
            if (id != 0)
            {
                using (labDB d = new labDB())
                {
                    d.SingleInto(this, " where sampleid = @0", id);
                }
                return;
            }
            Tech = "";
            Note = "";
            ProductCodeId = 0;
            Stamp = DateTime.Now;
            ParameterId = _type;
        }

        /// <summary>
        /// Get production context of sample using timestamp and line number.
        /// Also catch retests using reel number and footage.
        /// </summary>
        /// <returns></returns>
        public bool Synchronize()
        {
            var time = Scheduled.ToStamp();

            var s = new Sample();
            using (labDB d = new labDB())
            {
                _isUpdating = false;
                s = d.SingleOrDefaultInto(this, " where scheduled = @0 and lineid = @1 and reelnumber = @2 and footage = @3", time, LineId, ReelNumber, Footage);
                if (s != null)
                {
                    _isUpdating = true;
                    return true;                            // sample already exists
                }

                var context = d.SingleOrDefault<LineTx>(LineTx.contextByLine(time), LineId);
                if (context == null)
                {
                    return false;                           // cannot find context
                }
                ProductCodeId = context.ProductCodeId;      // apply context
                SystemId = context.SystemId;
                return true;
            }
        }

        public CasingSample(IRow r, DateTime sampled, string tech) : base(0)
        {
            Stamp = sampled;
            Scheduled = sampled;
            Note = "";
            Tech = tech.Replace(".", "").Trim().Substring(0, 2);
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
            //Debug.WriteLine("line: " + line + ", LineId: " + LineId);
            if (!Synchronize())
            {
                LineId = 0;         // context not found
                return;
            }

            var meter = r.GetCell(1).StringCellValue.Replace("--", "-").Split('-');

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

            try
            {
                var delmcell = r.GetCell(2);
                Delm = (delmcell.CellType == CellType.Numeric) ? (int)delmcell.NumericCellValue : 0;
                var rotocell = r.GetCell(13);
                Roto = (rotocell.CellType == CellType.Numeric) ? (int)rotocell.NumericCellValue : 0;                    // sometimes a text cell
                var oilcell = r.GetCell(12);
                OilPct = (oilcell?.CellType == CellType.Numeric) ? (int)(oilcell.NumericCellValue * 1000.0) : 0;

                ParameterId = _type;

                Save();
            }
            catch (Exception e)
            {
                Debug.WriteLine("saving CasingSample " + CasingSamples.current + ", " + LineId + ": " + e.Message + "\n" + e.StackTrace);
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
                Gly.ParameterId = _type;
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
            try
            {
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

                if (GlyStatus.Contains("control")) r.GetCell(10).CellStyle = CasingSamplesView.yellow;
                else if (GlyStatus.Contains("spec")) r.GetCell(10).CellStyle = CasingSamplesView.red;

                if (MoistStatus.Contains("control")) r.GetCell(5).CellStyle = CasingSamplesView.yellow;
                else if (MoistStatus.Contains("spec")) r.GetCell(5).CellStyle = CasingSamplesView.red;
            }
            catch (Exception e) { Debug.WriteLine(Line.Name + "error: " + e.Message + "\nstack: " + e.StackTrace); }
        }
    }

    public class CasingBatch
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

        public CasingBatch() { }
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
        public List<CasingBatch> batches { get; set; }
        public HttpPostedFileBase file { get; set; }

        public CasingSamples()
        {
            using (labDB d = new labDB())
            {
                d.OneTimeCommandTimeout = 120;
                batches = d.Fetch<CasingBatch>(_all);
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
        public static Tuple<int?, DateTime?> ReadExcel(Stream file, bool publish, int year)
        {
            var now = DateTime.Now;
            int day = now.DayOfYear;

            int lastid = 0;
            int samplecount = 0;
            DateTime stamp = DateTime.MinValue;
            List<int> addIds = new List<int>();
             
            IWorkbook hssfwb = null;
            try
            {
                hssfwb = WorkbookFactory.Create(file);
            }
            catch (Exception e)
            {
                return new Tuple<int?, DateTime?>(0, DateTime.MinValue);
            }
            string linerpt = "";
            try
            {
                ISheet s = hssfwb.GetSheet("Gly & Moist");
                var juliancell = s.GetRow(2).GetCell(2);
                var shiftcell = s.GetRow(2).GetCell(4);
                int julian, shift;
                string[] parsed = { "", "" };

                if (juliancell.CellType == CellType.Blank || shiftcell.CellType == CellType.Blank)
                {
                    var fs = file as FileStream;
                    if (fs == null) return null;
                    parsed = Path.GetFileNameWithoutExtension(fs.Name).Split('-');
                }

                if (juliancell.CellType == CellType.Blank)
                {
                    if (!int.TryParse(parsed[0], out julian))   return null;
                    if (julian < 1 || julian > 366)             return null;
                }
                else
                    julian = (int)juliancell?.NumericCellValue;

                if (shiftcell.CellType == CellType.Blank)
                {
                    if (!int.TryParse(parsed[1], out shift))    return null;
                    if (shift < 1 || shift > 4)                 return null;
                }
                else
                    shift = (int)shiftcell.NumericCellValue;


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
                    linerpt = "row: " + row + " ";
                    Debug.Write(linerpt);
                    var c = new CasingSample(s.GetRow(row), stamp, tech);
                    linerpt = "line: " + c.LineId + ", row: " + row + ", stamp: " + stamp + (c.readings == null ? "" : ", readings: " + c.readings.Count());
                    Debug.WriteLine(linerpt);
                    if (c.LineId == 0)
                        continue;
                    lastid = c.SampleId;
                    addIds.Add(c.SampleId);
                    samplecount++;
                }
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Readexcel: ", e));
                Debug.WriteLine("(" + current + ", " + linerpt + ", lastid: "+lastid+") exception in ReadExcel: " + e.Message);
                Debug.WriteLine(e.StackTrace);
                Debug.WriteLine(e.Data["LastSQL"]);
                return null;
            }
            if (publish)
            {
                var ids = string.Join(",", addIds.Select(i => i));
                CasingSampleArchive.complete(ids);
                CasingSampleArchive.publish(ids);
            }

            return new Tuple<int?, DateTime?>(samplecount, stamp);
        }

        public static string current = "";

        private static Tuple<int, int> readExcels(string path, string pattern, int year)
        {
            int filecount = 0;
            int samplecount = 0;

            var files = Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);

            foreach (var set in files)
            {
                using (FileStream file = new FileStream(set, FileMode.Open, FileAccess.Read))
                {
                    current = set;
                    if (file.Name.ToLower().Contains("qa"))
                    {
                        Debug.WriteLine("skipping " + file.Name);
                        continue;
                    }
                    Debug.WriteLine("file: " + file.Name);
                    Debug.Indent();

                    var count = ReadExcel(file, true, year);
                    Debug.Unindent();
                    if (count == null)
                    {
                        Debug.WriteLine("skipping " + file.Name);
                        continue;
                    }
                    samplecount += count.Item1.HasValue ? count.Item1.Value : 0;
                    if (count.Item1.HasValue) filecount++;
                }
            }
            return new Tuple<int, int>(filecount, samplecount);
        }

        public static string ReadExcels(string path, int year)
        {
            int filecount = 0;
            int samplecount = 0;

            var t = readExcels(path, "*.xls", year);

            filecount = t.Item1;
            samplecount = t.Item2;

            t = readExcels(path, "*.xlsx", year);

            filecount += t.Item1;
            samplecount += t.Item2;

            return filecount + " files read, " + samplecount + " samples recorded";
        }
    }

    public class CasingSamplesView
    {
        public string Symbol { get { return CasingSample.Symbol; } }

        private static string _setproduct = @"
            update [sample] set productcodeid = {0} where sampleid = {1}
        ";

        public static string _batch = @"
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
                  ,n.[LineNumber]     as Line__LineNumber
                  ,n.[UnitId]         as Line__UnitId
                  ,r.[ReadingId]      as Gly__ReadingId     
                  ,r.[LineId]         as Gly__LineId        
                  ,r.[Stamp]          as Gly__Stamp         
                  ,r.[R1]             as Gly__R1            
                  ,r.[R2]             as Gly__R2                
                  ,r.[R3]             as Gly__R3            
                  ,r.[R4]             as Gly__R4            
                  ,r.[R5]             as Gly__R5            
                  ,r.[Average]        as Gly__Average
                  ,r.[ParameterId]    as Gly__ParameterId
                  ,r.[Operator]       as Gly__Operator    
                  ,r.[EditCount]      as Gly__EditCount
                  ,r.[Scheduled]      as Gly__Scheduled     
                  ,r.[ParameterId]    as Gly__ParameterId
                  ,p.[ProductCodeId]  as _product__ProductCodeId      
                  ,p.[ProductCode]    as _product___ProductCode       
                  ,p.[ProductSpec]    as _product__ProductSpec       
                  ,p.[ReelMoist_Min]  as _product__ReelMoist_Min          
                  ,p.[ReelMoist_Aim]  as _product__ReelMoist_Aim          
                  ,p.[ReelMoist_Max]  as _product__ReelMoist_Max          
                  ,p.[Gly_Min]        as _product__Gly_Min
                  ,p.[Gly_Aim]        as _product__Gly_Aim               
                  ,p.[Gly_Max]        as _product__Gly_Max                   
                  ,p.[Oil_Min]        as _product__Oil_Min                   
                  ,p.[Oil_Aim]        as _product__Oil_Aim               
                  ,p.[Oil_Max]        as _product__Oil_Max  
                  ,p.[LF_Min]         as _product__LF_Min                   
                  ,p.[LF_Aim]         as _product__LF_Aim               
                  ,p.[LF_Max]         as _product__LF_Max  
                  ,p.[LF_UCL]         as _product__LF_UCL                   
                  ,p.[LF_LCL]         as _product__LF_LCL               
              FROM [dbo].[Sample] s
              left join [Line] n on n.LineId = s.LineId
              left join [dbo].[Reading] r on r.SampleId = s.SampleId
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
                    var time = ScheduleTime.ToStamp();
                    sset = d.Fetch<CasingSample>(_batch + _bytime, time);
                }
                else
                {
                    sset = d.Fetch<CasingSample>(_batch + _bysample, id);
                    ScheduleTime = sset.First().Scheduled;
                }
            }
            allCompleted = sset != null && sset.Count() > 0 && !sset.Any(s => (s.Completed ?? DateTime.MaxValue) > DateTime.Now);
            sset.Where(s => s.Gly != null).Select(s => s.Gly.parameter()).ToList();

            if (allCompleted)
            {
                samples = sset.ToLookup(k => k.Line.UnitId, v => v);
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

            samples = complete.ToLookup(k => k.Line.UnitId, v => v);
        }

        private static string _complete = @"
            update [sample] set completed = getdate() where sampleid in ({0})
        ";

        private static string _close = @"
            update [sample] set completed = getdate() where parameterid in ({0})
        ";

        // move results from lab database to tags database
        // calculations are performed during the move 
        // 
        private static string _insertlabresult = @"
            declare @@insertedtags table
            (
	            tagid int, 
	            sampleid int
            )

            declare @@archivecut datetime

            select @@archivecut = min(stamp) from [All]

            insert into [All] 
			output inserted.tagid, inserted.quality into @@insertedtags
	        select r.tagid,
	            cast(round((1 - l.r3 / l.r1) * 100.0, 1) as varchar(64)) as value,
	            l.stamp,
	            l.sampleid as quality
	        from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	        join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	        join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	        where f.FieldName = 'csg_moist_pct' 
		            and l.Completed is not null 
		            and l.sampleid in ({0})	
					and l.stamp >= @@archivecut

            insert into [All]
            output inserted.tagid, inserted.quality into @@insertedtags
	        select r.tagid,
                cast(round((l.r4 / l.r5 / 2.0 / ( l.r3 / l.r1 * l.r2 / 1000.0 * (1 - l.OilPct / 1000.0 ))) * 100.0, 1) as varchar(64)) as value,
	            l.stamp,
	            l.sampleid as quality
	        from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	        join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	        join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	        where f.FieldName = 'csg_glyc_pct' 
		        and l.Completed is not null 
		        and l.sampleid in ({0})	
				and l.stamp >= @@archivecut

            insert into [Past]
	        select r.tagid,
	            cast(round((1 - l.r3 / l.r1) * 100.0, 1) as varchar(64)) as value,
	            l.stamp
	        from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	        join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	        join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	        where f.FieldName = 'csg_moist_pct' 
		        and l.Completed is not null 
		        and l.sampleid in ({0})	
				and l.stamp < @@archivecut

            insert into [Past]
	        select r.tagid,
                cast(round((l.r4 / l.r5 / 2.0 / ( l.r3 / l.r1 * l.r2 / 1000.0 * (1 - l.OilPct / 1000.0 ))) * 100.0, 1) as varchar(64)) as value,
	            l.stamp
	        from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	        join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	        join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	        where f.FieldName = 'csg_glyc_pct' 
		        and l.Completed is not null 
		        and l.sampleid in ({0})	
				and l.stamp < @@archivecut

            update t 
            set t.relatedtagid = i.sampleid 
            from tag t
            join @@insertedtags i on t.tagid = i.tagid
        ";

        public void Seal()
        {
            var ids = string.Join(",", samples.SelectMany(x => x.Select(y => y.SampleId)).Where(k => k != 0).Distinct().ToArray());
            if (ids.Length == 0)
            {
                Debug.WriteLine("no ids");
                return;
            }
            Debug.WriteLine("sealing ids " + ids);
            using (labDB d = new labDB())
            {
                d.Execute(string.Format(_complete, ids));
            }
            using (tagDB t = new tagDB())
            {
                t.Execute(string.Format(_insertlabresult, ids));
            }
        }
    }
}