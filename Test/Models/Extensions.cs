using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public static class Extensions
    {
        /// <summary>
        /// Time related extensions for javascript and database
        /// </summary>
        private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        public static long ToJSMSecs(this DateTime dt)
        {
            return (long)((dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
        }

        public static DateTime FromJSMSecs(this double tm)
        {
            return new DateTime(((long)tm * 10000) + DatetimeMinTimeTicks).ToLocalTime();
        }


        public static string ToSStamp(this DateTime stamp)
        {
            return stamp.ToString("yyyy-MM-dd");
        }

        public static string Before(this DateTime stamp)
        {
            return stamp.ToSStamp() + " 00:00:00";
        }

        public static string ToStamp(this DateTime stamp)
        {
            return stamp.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToShort(this DateTime stamp)
        {
            return stamp.ToString("MM-dd HH:mm");
        }

        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// Converts a DateTime to its Unix timestamp value. This is the number of seconds
        /// passed since the Unix Epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="aDate">DateTime to convert</param>
        /// <returns>Number of seconds passed since 1/1/1970 UTC </returns>
        public static int ToInt(this DateTime aDate)
        {
            if (aDate == DateTime.MinValue)
            {
                return -1;
            }
            TimeSpan span = (aDate - UnixEpoch);
            return (int)Math.Floor(span.TotalSeconds);
        }

        /// <summary>
        /// Converts the specified 32 bit integer to a DateTime based on the number of seconds
        /// since the Unix epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="anInt">Integer value to convert</param>
        /// <returns>DateTime for the Unix int time value</returns>
        public static DateTime ToDateTime(this int anInt)
        {
            if (anInt == -1)
            {
                return DateTime.MinValue;
            }
            return UnixEpoch.AddSeconds(anInt);
        }

        /// <summary>
        /// Specification related functions
        /// </summary>
        /// <param name="cs">Casing Sample</param>
        /// <param name="pct">Projection onto either Moisture, Glycerin or Oil</param>
        /// <returns>Rounded return for equivalence to Excel programming</returns>
        public static double[] specs(this CasingView cs, Func<CasingView, double?> pct)
        {
            if (cs == null || cs.product == null) return null;
            if (pct(cs) == cs.MoistPct)
                return new double[]
                {
                    Math.Round(cs.product.ReelMoist_Min.Value, 1),
                    Math.Round(cs.product.ReelMoist_Aim.Value, 1),
                    Math.Round(cs.product.ReelMoist_Max.Value, 1)
                };
            if (pct(cs) == cs.GlyPct)
                return new double[]
                {
                    Math.Round(cs.product.Gly_Min.Value, 1),
                    Math.Round(cs.product.Gly_Aim.Value, 1),
                    Math.Round(cs.product.Gly_Max.Value, 1)
                };
            if (pct(cs) == cs.LayFlat)
                return new double[]
                {
                    Math.Round(cs.product.LF_Min.Value, 1),
                    Math.Round(cs.product.LF_LCL.Value, 1),
                    Math.Round(cs.product.LF_Aim.Value, 1),
                    Math.Round(cs.product.LF_UCL.Value, 1),
                    Math.Round(cs.product.LF_Max.Value, 1)
                };
            return null;
        }

        public static bool OOSpec(this CasingView cs, Func<CasingView, double?> pct)
        {
            if (cs == null || !pct(cs).HasValue) return false;
            var std = cs.specs(pct);
            if (std == null) return false;
            var val = Math.Round(pct(cs).Value, 1);
            return val < std[0] || std[std.Length-1] < val;
        }

        public static bool OOControl(this CasingView cs, Func<CasingView, double?> pct)
        {
            if (cs == null || !pct(cs).HasValue) return false;
            var std = cs.specs(pct);
            if (std == null) return false;
            var val = Math.Round(pct(cs).Value, 1);
            if (pct(cs) == cs.MoistPct)          return val < (std[0] + .5) || (std[2] - .5) < val;
            if (pct(cs) == cs.GlyPct)            return val < (std[1] - 1) || (std[1] + 1) < val;
            if (pct(cs) == cs.LayFlat)           return val < std[1] || std[3] < val;
            return false;
        }

        public static string SpecColor(this CasingView cs, Func<CasingView, double?> pct)
        {
            if (cs.OOSpec(pct)) return "danger";
            if (cs.OOControl(pct)) return "warning";
            return "";
        }
    }
}