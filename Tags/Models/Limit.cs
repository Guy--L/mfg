using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public partial class Limit
    {
        private const double clipmargin = 10.0;

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
            order by tagid, stamp
        ";
        
        public static List<Limit> Specs(tagDB t, string tags, DateTime minstamp, DateTime maxstamp)
        {
            return t.Fetch<Limit>(string.Format(_limits, tags, minstamp.ToStamp(), maxstamp.ToStamp()));
        }

        public string Clip(string value)
        {
            double dbl = 0.0;
            if (!double.TryParse(value, out dbl))
                return "0.0";
            if (LoLo - clipmargin <= dbl && dbl <= HiHi + clipmargin)
                return dbl.ToString();
            if (dbl < LoLo - clipmargin)
                return (LoLo - clipmargin).ToString();
            return (HiHi + clipmargin).ToString();
        }
    }
}