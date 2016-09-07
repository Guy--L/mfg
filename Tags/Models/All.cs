using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
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