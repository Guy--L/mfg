using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class CasingSampleArchive
    {
        private static string _complete = @"
            update [sample] set completed = getdate() where stamp <= '{0}' and year(stamp) = {1}
        ";

        private static string _completeIds = @"
            update [sample] set completed = getdate() where sampleid in ({0})
        ";

        // move results from lab database to tags database
        // calculations are performed during the move 
        // 
        private static string _labresult = @"
            declare @@archivecut datetime

            select @@archivecut = min(stamp) from [All]

            insert into [All] (tagid, value, stamp, quality)
	        select r.tagid,
	            cast(round((1 - l.r3 / l.r1) * 100.0, 1) as varchar(64)) as value,
	            l.stamp,
	            l.sampleid as quality
	            from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	            join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	            join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	            where f.FieldName = 'csg_moist_pct' 
		            and l.Completed is not null 
                    and {0}
                    and l.stamp >= @@archivecut

            insert into [All] (tagid, value, stamp, quality)
            select r.tagid,
                cast(round((l.r4 / l.r5 / 2.0 / ( l.r3 / l.r1 * l.r2 / 1000.0 * (1 - l.OilPct / 1000.0 ))) * 100.0, 1) as varchar(64)) as value,
	            l.stamp,
	            l.sampleid as quality
	            from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	            join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	            join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	            where f.FieldName = 'csg_glyc_pct' 
		            and l.Completed is not null 
		            and {0}
                    and l.stamp >= @@archivecut

            ;with avgs as (
	            select [sys], convert(varchar(64), avg(gly), 0) as value, g.stamp, 192 as quality from (
		            select a.stamp, convert(float, a.value, 1) as gly, 'Sys'+convert(char(1), l.systemid, 1) as [sys]
		            from [all] a
		            join tag t on t.tagid = a.tagid
		            join mesdb.dbo.[sample] l on l.sampleid = a.quality
		            where t.name = 'csg_glyc_pct' 
                        and len(a.value) < 5
                        and {0}
	            ) g
	            group by stamp, [sys]
            )
            insert into [All] 
            select t.TagId, v.value, v.stamp, v.quality
            from avgs v 
            join channel c on c.Name = v.[sys]
            join device d on c.ChannelId = d.ChannelId
            join tag t on t.DeviceId = d.DeviceId
            where t.name = 'csg_glyc_pct'

            insert into [Past] (tagid, value, stamp)
            select r.tagid,
	            cast(round((1 - l.r3 / l.r1) * 100.0, 1) as varchar(64)) as value,
	            l.stamp
	            from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	            join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	            join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	            where f.FieldName = 'csg_moist_pct' 
		            and l.Completed is not null 
                    and {0}
                    and l.stamp < @@archivecut

            insert into [Past] (tagid, value, stamp)
            select r.tagid,
                cast(round((l.r4 / l.r5 / 2.0 / ( l.r3 / l.r1 * l.r2 / 1000.0 * (1 - l.OilPct / 1000.0 ))) * 100.0, 1) as varchar(64)) as value,
	            l.stamp
	            from mesdb.dbo.[LabResult] l                                                -- this is a view not a table
	            join mesdb.dbo.[ReadingTag] r on  r.LineId = l.LineId
	            join mesdb.dbo.[ReadingField] f on r.ReadingFieldId = f.ReadingFieldId 
	            where f.FieldName = 'csg_glyc_pct' 
		            and l.Completed is not null 
		            and {0}
                    and l.stamp < @@archivecut
        ";

        public static string _year = @" l.stamp <= '{0}' and year(l.stamp) = {1} ";
        public static string _ids = @" l.sampleid in ({0}) ";

        public static void complete(int yr)
        {
            using (labDB d = new labDB())
            {
                var t = d.Execute(string.Format(_complete, DateTime.Now.ToStamp(), yr));
            }
        }

        public static void complete(string ids)
        {
            using (labDB d = new labDB())
            {
                var t = d.Execute(string.Format(_completeIds, ids));
            }
        }

        public static void publish(int yr)
        {
            using (tagDB d = new tagDB())
            {
                var query = string.Format(_labresult, _year);
                query = string.Format(query, DateTime.Now.ToStamp(), yr);
                var t = d.Execute(query);
            }
        }

        public static void publish(string ids)
        {
            using (tagDB d = new tagDB())
            {
                var query = string.Format(_labresult, _ids);
                query = string.Format(query, ids);
                var t = d.Execute(query);
            }
        }
    }
}