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

            alter table [All] disable trigger SyncValue2Current

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

            alter table [All] enable trigger SyncValue2Current

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
                var t = d.Execute(string.Format(_complete, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), yr));
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
                query = string.Format(query, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), yr);
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