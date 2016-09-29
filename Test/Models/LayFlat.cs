using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class LayFlats
    {
        private static string _all = @"
            
        ";
        public DateTime horizon { get; set; }
        public List<LayFlat> list { get; set; }
        public int sample { get; set; }

        public LayFlats()
        {
            horizon = DateTime.Now.AddDays(-14);
            using (labDB d = new labDB())
            {
                list = d.FetchOneToMany<LayFlat>(p => p.readings, Sample._sample + " where b.Stamp > @0 and b.ParameterId = @1", horizon.ToStamp(), LayFlat._type).Select(n => n.specialize()).ToList();
            }
            sample = list.Any() ? list.First().Type.Count : 0;
        }
    }

    public class LayFlat : Sample
    {
        private static string layFlat = "LayFlat";
        public static int _type = Parameter.TypeOf[layFlat];
        public static string Symbol = Parameter.IconML(_type);
        public static string Mask = Parameter.Types[_type].Mask;

        public double Average { get; set; }
        public double Range { get; set; }
        public string SpecColor { get; set; }
        public string ScheduledJ { get { return Scheduled.ToJulian(); } }

        public List<ReadItem> values { get; set; }

        public LayFlat() { }

        public LayFlat specialize()
        {
            var sum = 0.0;
            var max = -1.0;
            var min = (double)Product.LF_Max * 10;
            Type = Parameter.Types[ParameterId.Value];
            values = Enumerable.Range(0, Type.Count).Select(r => {
                var s = readings[r / Reading.CountPerRecord][r % Reading.CountPerRecord];
                var v = double.Parse(s);
                if (v > max) max = v;
                if (v < min) min = v;
                sum += v;
                return new ReadItem(v.ToString("F"+Type.Scale), Type.Mask);
            }).ToList();
            Average = sum / Type.Count;
            Range = max - min;
            return this;
        }

        public static new LayFlat Existing(int id)
        {
            var layflat = repo.FetchOneToMany<LayFlat>(p => p.readings, _sample + "where b.[SampleId] = @0", id).SingleOrDefault();
            layflat.specialize();
            return layflat;
        }

        public LayFlat(bool children) : base("")
        {
            values = Enumerable.Range(0, Type.Count).Select(v => new ReadItem(Type.Mask)).ToList();
            Average = Range = 0.0;
        }

        public void Save(object session)
        {
            List<Reading> readings = session as List<Reading>;
            ParameterId = _type;

            Note = "";

            if (readings == null) Structure();
            Stamp = DateTime.Now;              
            base.Save();
            values.Select((v, i) => { readings[i / Reading.CountPerRecord][i % Reading.CountPerRecord] = v.Value; return 1; }).ToList();
            readings.ForEach(r => { r.SampleId = SampleId; r.Stamp = Stamp; r.Scheduled = Scheduled; r.Operator = Tech; r.Save(); });
        }
    }

    public class LayFlatView
    {
        public LinePicker lines { get; set; }
        public LayFlat lf { get; set; }
        public List<ReadItem> readings { get; set; }
        public int linewidth { get; set; }

        public LayFlatView() { }

        public LayFlatView(int id)
        {
            lf = id == 0 ? new LayFlat(true) : LayFlat.Existing(id);
            lines = new LinePicker(new Lines());
        }
    }
}