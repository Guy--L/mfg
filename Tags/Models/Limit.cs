using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public partial class Limit
    {
        private const double clipmargin = 2.0;

        public double ClipLo { get { return (Aim - 2 * (Aim - LoLo)); } }
        public double ClipHi { get { return (Aim + 2 * (HiHi - Aim)); } } 

        public static string _limits = @"
            select limitid, tagid, '{1}' stamp, lolo, lo, aim, hi, hihi from (
             select limitid, tagid, stamp, lolo, lo, aim, hi, hihi, 
              ROW_NUMBER() OVER (PARTITION BY tagid ORDER BY stamp DESC) rn
             from limit
             where stamp < '{1}' and tagid in ({0})
            ) tmp where rn = 1
            union
            select limitid, tagid, stamp, lolo, lo, aim, hi, hihi from limit
            where stamp >= '{1}' and stamp <= '{2}' and tagid in ({0})
            union
            select limitid, tagid, '{2}' stamp, lolo, lo, aim, hi, hihi from (
             select limitid, tagid, stamp, lolo, lo, aim, hi, hihi, 
              ROW_NUMBER() OVER (PARTITION BY tagid ORDER BY stamp DESC) rn
             from limit
             where stamp <= '{2}' and tagid in ({0})
            ) tmp where rn = 1
            order by tagid, stamp
        ";
        
        public static List<Limit> Specs(tagDB t, string tags, DateTime minstamp, DateTime maxstamp)
        {
            return t.Fetch<Limit>(string.Format(_limits, tags, minstamp.ToStamp(), maxstamp.ToStamp()));
        }

        public string Clip(string value)
        {
            double loclip = Aim - 2 * (Aim - LoLo);
            double dbl = 0.0;
            if (!double.TryParse(value, out dbl))
                return "0.0";
            if (ClipLo <= dbl && dbl <= ClipHi)
                return dbl.ToString();
            if (dbl < ClipLo)
                return ClipLo.ToString();
            return ClipHi.ToString();
        }

        public object Clip(object value)
        {
            double dbl = (double)value;
            if (ClipLo <= dbl && dbl <= ClipHi) return dbl;
            return dbl < ClipLo ? ClipLo : ClipHi;
        }

        public override string ToString()
        {
            return string.Format("{0,12:MM/dd HH:mm} {1:N2} {2:N2} {3:N2} {4:N2} {5:N2}", Stamp, LoLo, Lo, Aim, Hi, HiHi);
        }
    }
}