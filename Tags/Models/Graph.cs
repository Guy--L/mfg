using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public partial class Graph
    {
        private static string _updview = @";
            declare @@graphid as int
            declare @@tagname as table(
                [Id] int,
                [Device] [varchar](64),
                [Name] [varchar](64)
            ), @@userid as int

            insert into @@tagname 
            select t.TagId, d.Name, t.Name 
            from Tag t 
            join Device d on d.DeviceId = t.DeviceId
            where t.TagId in ({0})

            Merge [user] u
            using (select [Login] = @0) s 
            ON s.[Login] = u.[Login]
            WHEN NOT matched THEN 
            INSERT ([Login], [Identity]) VALUES (s.[Login], @1);

            select @@userid = userid from [user] where [Login] = @0

            merge [graph] g
            using (select [Name] = @2) n
            on n.[Name] = g.[GraphName] and g.[UserId] = @@userid
            when not matched then 
            insert ([GraphName], [UserId], [Shared])
            values (n.[Name], @@userid, 0);

            select @@graphid = graphid from [graph] where graphname = @2 and userid = @@userid

            delete p
            from [plot] p
            join [tag] t on t.tagid = p.tagid
            join [device] d on d.deviceid = t.deviceid
            left join @@tagname n on t.name = n.name and d.name = n.device
            where n.name is null and p.graphid = @graphid

            insert [plot]
            select @@graphid, t.id, 0, t.name, 1, -1, -1
            from @@tagname t
            left join [plot] p on t.id = p.tagid and p.graphid = @@graphid
            where p.plotid is null
        ";

        private static string _graphsByUser = @"
            select g.graphname
                ,g.shared
                ,p.plotid
                ,p.tagid
                from graph g
                join plot p on g.graphid = p.plotid
                where g.userid = @0
        ";

        private static string _deleteView = @"
            delete p
            from plot p
            where p.graphid in (@0)

            delete g 
            from graph g 
            where g.graphid in (@0)
        ";

        public static void DeleteView(string ids)
        {
            using (tagDB t = new tagDB())
            {
                t.Execute(_deleteView, ids);
            }
        }

        public static void SaveView(User user, string name, int[] tagids)
        {
            var ids = string.Join(",", tagids);
            using (tagDB t = new tagDB())
            {
                t.Execute(";exec dbo.UpdatePlot @0, @1, @2, @3", user.Login, user.Identity, name, ids);
            }
        }
    }
}