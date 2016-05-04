using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Tags.Models
{
    public partial class Plot
    {
        private static string _plotsByUser = @";
            Merge [user] u
            using (select [Login] = '{0}') s 
            ON s.[Login] = u.[Login]
            WHEN NOT matched THEN 
            INSERT ([Login], [Identity]) VALUES (s.[Login], '');

            select
            c.GraphId,
            c.GraphName,
            s.PlotId, 
            s.TagId, 
            s.YAxis,
            s.Relabel,
            s.Scale,
            s.MinY,
            s.MaxY,
            t.Name Path
            from [Plot] s
            join [Current] t on t.TagId = s.Tagid
            join [Graph] c on s.GraphId = c.GraphId
            join [User] u on c.UserId = u.UserId
            where u.[Login] = '{0}'
        ";

        [Reference(ReferenceType.OneToOne, ColumnName = "GraphId", ReferenceMemberName = "GraphId")]
        [Column] public Graph graph { get; set; }
        [ResultColumn] public string Path { get; set; }

        public List<Tag> tags { get; set; }
        public Tag filter { get; set; }

        public static ILookup<string, Plot> plotsByUser(string user)
        {
            ILookup<string, Plot> results;

            using (tagDB tdb = new tagDB())
            {
                var res = tdb.Fetch<Plot>(string.Format(_plotsByUser, user));
                results = res.ToLookup(k => k.graph.GraphName);
            }
            return results;
        }

        public static ILookup<string, int> chartTagsByUser(string user)
        {
            ILookup<string, int> results;

            using (tagDB tdb = new tagDB())
            {
                var res = tdb.Fetch<Plot>(string.Format(_plotsByUser, user));
                results = res.ToLookup(k => k.graph.GraphName, m => m.TagId);
            }
            return results;
        }

    }
}