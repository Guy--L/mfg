using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public partial class Limit
    {
        public static string _limits = @"
            select limitid, tagid, stamp, lolo, lo, aim, hi, hihi from (
             select limitid, tagid, stamp, lolo, lo, aim, hi, hihi, 
              ROW_NUMBER() OVER (PARTITION BY tagid ORDER BY stamp DESC) rn
             from limit
             where stamp < '{1}' and tagid in ({0})
            ) tmp where rn = 1
            union
            select limitid, tagid, stamp, lolo, lo, aim, hi, hihi from limit
            where stamp >= '{1}' and stamp <= '{2}'
            order by tagid, stamp
        ";
        
        public static List<Limit> Specs(tagDB t, string tags, DateTime minstamp, DateTime maxstamp)
        {
            return t.Fetch<Limit>(string.Format(tags, minstamp.ToString("yyyy-MM-dd HH:mm:ss"), maxstamp.ToString("yyyy-MM-dd HH:mm:ss")));
        }
    }
}