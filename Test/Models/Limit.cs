using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public partial class Limit
    {
        private static string _deleteLimits = @"
            declare @@deleted int

            delete lm 
            from taglogs.dbo.[Limit] lm
            join readingtag r on r.tagid = lm.tagid
            join readingfield f on f.readingfieldid = r.readingfieldid
            join line n on n.lineid = r.lineid
            where n.lineid in ({0}) and lm.stamp = n.stamp and f.fieldname in ('csg_moist_pct','csg_glyc_pct')

            select @@deleted = @@@rowcount

            delete lm
            from taglogs.dbo.[Limit] lm
		    join taglogs.dbo.[Tag] t on t.tagid = lm.tagid
		    join taglogs.dbo.[Device] d on d.deviceid = t.deviceid
		    join taglogs.dbo.[Channel] c on d.channelid = c.channelid
		    join [Unit] u on u.unit = left(c.name, 1)
		    join [Line] i on i.linenumber = substring(c.name, 2, 1)
		    where t.name = 'layflat_mm_pv' and d.name = 'Dry' and lm.stamp = i.stamp and i.lineid in ({0})

            select @@@rowcount + @@deleted;
        ";

        private static string _insertLimits = @"
            declare @@inserted int
    
		    insert into taglogs.dbo.[Limit]
		    select distinct r.tagid, {1}
			    , coalesce(p.reelmoist_min,0)
			    , coalesce(p.reelmoist_min + 0.5,0)
			    , coalesce(p.reelmoist_aim,0)
			    , coalesce(p.reelmoist_max - 0.5,0)
			    , coalesce(p.reelmoist_max,0)
		    from [ReadingTag] r
		    join [ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
		    join [Line] i on i.lineid = r.LineId
		    join [ProductCode] p on p.ProductCodeId = i.productcodeid
		    where f.FieldName = 'csg_moist_pct' and i.lineid in ({0})

            select @@inserted = @@@rowcount

            insert into taglogs.dbo.[Limit]
		    select distinct r.tagid, {1}
			    , coalesce(p.gly_min,0)
			    , coalesce(p.gly_aim - 1.0,0)
			    , coalesce(p.gly_aim,0)
			    , coalesce(p.gly_aim + 1.0,0)
			    , coalesce(p.gly_max,0)
		    from [ReadingTag] r
		    join [ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
		    join [Line] i on i.lineid = r.LineId
		    join [ProductCode] p on p.ProductCodeId = i.productcodeid
		    where f.FieldName = 'csg_glyc_pct' and i.lineid in ({0})

            select @@inserted = @@@rowcount + @@inserted

            insert into taglogs.dbo.[Limit]
		    select distinct t.tagid, {1}
			    , coalesce(p.LF_Min,0)
			    , coalesce(p.LF_LCL,0)
			    , coalesce(p.LF_Aim,0)
			    , coalesce(p.LF_UCL,0)
			    , coalesce(p.LF_Max,0)
		    from [ProductCode] p 
		    join [Line] i on p.ProductCodeId = i.productcodeid
		    join [Unit] u on u.unitid = i.UnitId
		    join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
		    join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
		    join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
		    where t.name = 'layflat_mm_pv' and d.name = 'Dry'

            select @@@rowcount + @@inserted
        ";

        private static string _current = @"
            SELECT c.name+'.'+d.name+'.'+t.name name, t1.*
            FROM limit t1
            join tag t on t1.tagid = t.tagid
            join device d on t.deviceid = d.DeviceId
            join channel c on d.ChannelId = c.ChannelId
            LEFT OUTER JOIN limit t2 ON (t1.tagid = t2.tagid AND t1.stamp < t2.stamp)
            where t2.tagid is null
            order by name
        ";
        
        public static string UpdateLimits(string lineids, bool replace)
        {
            using (var db = new labDB())
            {
                int deleted = 0;
                string error = "";

                try {
                    if (replace)
                        deleted = db.ExecuteScalar<int>(string.Format(_deleteLimits, lineids));

                    error = deleted + " deleted old limit records. ";
                    var now = DateTime.Now.ToStamp();
                    var query = string.Format(_insertLimits, lineids, replace ? "i.stamp" : "'" + now + "'");
                    var inserted = db.ExecuteScalar<int>(query);

                    return deleted + " deleted old limit records, " + inserted + " inserted new limit records.";
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(db.LastSQL, e));
                    return "error in UpdateLimits: " + error + e.Message;
                }
            }
        }
    }
}