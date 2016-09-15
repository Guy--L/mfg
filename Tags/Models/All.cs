using System;
using NPoco;

namespace Tags.Models
{
    public class Value : All
    {
        [ResultColumn] public long prdid { get; set; }
        [ResultColumn] public double? dvalue { get; set; }
        [ResultColumn] public long epoch { get; set; }
        [ResultColumn] public int ctrl { get; set; }

        public string print()
        {
            return TagId + " " + Stamp.ToStamp() + ", " + epoch + " " + dvalue;
        }
    }

    /// <summary>
    /// Little class for json serialization of gobs of data
    /// </summary>
    public class Val
    {
        public long t { get; set; }
        public double y { get; set; }

        public Val(Value v)
        {
            t = v.epoch;
            y = v.dvalue ?? 0;
        }

        public Val(long e, double v)
        {
            t = e;
            y = v;
        }
    }

    public partial class All
    {
        private static string _horizon = @"
            select min(stamp) from [all]
        ";

        private static DateTime _firststamp;

        public static DateTime FirstStamp
        {
            get {
                if (_firststamp == null)
                    GetFirstStamp();
                return _firststamp;
            }
        }

        public static void GetFirstStamp()
        {
            using (tagDB t = new tagDB())
                _firststamp = t.ExecuteScalar<DateTime>(_horizon);
        }
    }
}