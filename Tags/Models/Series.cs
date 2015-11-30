using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Tags.Models
{
    public partial class Series
    {
        private static string _implicitAddUser = @"
            Merge [user] u
            using (select [Identity] = '{0}') s 
            ON s.[Identity] = u.[Identity]
            WHEN NOT matched THEN 
            INSERT ([Identity]) VALUES (s.[Identity]);
        ";
        private static string _seriesByUser = @";
            Merge [user] u
            using (select [Login] = '{0}') s 
            ON s.[Login] = u.[Login]
            WHEN NOT matched THEN 
            INSERT ([Login], [Identity]) VALUES (s.[Login], '');

            select
            c.ChartId,
            c.ChartName,
            s.SeriesId, 
            s.TagId, 
            s.YAxis,
            s.Relabel,
            s.Scale,
            s.MinY,
            s.MaxY
            from Series s
            join Chart c on s.ChartId = c.ChartId
            join UserChart x on x.ChartId = c.ChartId
            join [User] u on x.UserId = u.UserId
            where u.[Login] = '{0}'
        ";

        [ResultColumn] public string ChartName { get; set; }

        public static ILookup<string, Series> seriesByUser(string user)
        {
            ILookup<string, Series> results;

            using (tagDB tdb = new tagDB())
            {
                var res = tdb.Fetch<Series>(string.Format(_seriesByUser, user));
                results = res.ToLookup(k => k.ChartName);
            }
            return results;
        }

        public static ILookup<string, int> chartTagsByUser(string user)
        {
            ILookup<string, int> results;

            using (tagDB tdb = new tagDB())
            {
                var res = tdb.Fetch<Series>(string.Format(_seriesByUser, user));
                results = res.ToLookup(k => k.ChartName, m => m.TagId);
            }
            return results;
        }

        //public static IEnumerable<Series> seriesByUser(string user)
        //{
        //    IEnumerable<Series> res;
        //    using (tagDB tdb = new tagDB())
        //    {
        //        res = tdb.Fetch<Series>(string.Format(_seriesByUser, user));
        //    }
        //    return res;
        //}
    }
}