--declare @stamp datetime
--declare @unitid int
--declare @systemid int
--declare @statusid int
--declare @productcodeid int
--declare @lineid int
 update n set rscode = 'RP' from lineoperation n where n.stcode = 'U' and n.rscode != 'RP'		and year(n.stamp) > 2014
 update n set rscode = 'DN' from lineoperation n where n.rscode = 'D'							and year(n.stamp) > 2014
 update n set rscode = 'PM' from lineoperation n where n.rscode in ('PF', 'RM')					and year(n.stamp) > 2014
 update n set rscode = 'DN' from lineoperation n where rtrim(n.rscode) = ''						and year(n.stamp) > 2014

-- use a cursor here because we want trigger to insert the product codes, line status changes and spec information into the tag database.
-- also linetx will be added by the trigger.  A normal update query does not step through all the transactions but optimizes to the last 
-- effective update to the line table.

USE [mesdb]

declare @newinsert table (
	ltxid int,
	stamp datetime,
	lineid int,
	productcodeid int
)

insert into linetx
output inserted.LineTxId, inserted.stamp, inserted.lineid, inserted.productcodeid into @newinsert
	select n.lineid, 0 as personid, n.stamp, 
	(select top 1 comment
		from [plan]
		where lineid = n.lineid
		and productcodeid = n.ProductCodeId
		and stamp <= n.stamp
		order by stamp desc) 
	as comment,
	0 as linetankid, u.unitid, l.linenumber, 
	(select top 1 systemid
		from [plan]
		where lineid = n.lineid
		and productcodeid = n.ProductCodeId
		and stamp <= n.stamp
		order by stamp desc) 
	as systemid,
		s.statusid, 
		n.ProductCodeId
	from lineoperation n
	join line l on n.lineid = l.lineid
	join unit u on u.unitid = l.unitid
	join [status] s on s.Code = n.rscode
	where year(n.stamp) > 2014

update n
set n.recordid = i.ltxid
from lineoperation n
join @newinsert i on i.lineid = n.lineid and i.stamp = n.stamp and i.productcodeid = n.productcodeid

use [taglogs]

declare @archivecut datetime
select @archivecut = min(stamp) from [all]

insert into [all]
select t.tagid, p.productcode+' '+p.productspec as product, i.stamp, 192 as quality
from mesdb.dbo.linetx i
join mesdb.dbo.productcode p on i.ProductCodeId = p.ProductCodeId
join mesdb.dbo.line l on i.lineid = l.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join mesdb.dbo.[Status] s on s.StatusId = i.statusid
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'product_code' and d.Name = 'HMI' 
and i.stamp >= @archivecut and s.Code = 'RP'

insert into [past]
select t.tagid, p.productcode+' '+p.productspec as product, i.stamp
from mesdb.dbo.linetx i
join mesdb.dbo.productcode p on i.ProductCodeId = p.ProductCodeId
join mesdb.dbo.line l on i.lineid = l.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join mesdb.dbo.[Status] s on s.StatusId = i.statusid
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'product_code' and d.Name = 'HMI' 
and i.stamp < @archivecut and s.Code = 'RP'

insert into taglogs.dbo.[Limit]
select distinct r.tagid, i.stamp
	, coalesce(p.reelmoist_min,0)
	, coalesce(p.reelmoist_min + 0.5,0)
	, coalesce(p.reelmoist_aim,0)
	, coalesce(p.reelmoist_max - 0.5,0)
	, coalesce(p.reelmoist_max,0)
from mesdb.dbo.[ReadingTag] r
join mesdb.dbo.[ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
join mesdb.dbo.linetx i on i.lineid = r.LineId
join mesdb.dbo.[ProductCode] p on p.ProductCodeId = i.productcodeid
join mesdb.dbo.[Status] s on s.StatusId = i.statusid
where f.FieldName = 'csg_moist_pct' and s.Code = 'RP'

insert into taglogs.dbo.[Limit]
select distinct r.tagid, i.stamp
	, coalesce(p.gly_min,0)
	, coalesce(p.gly_aim - 1.0,0)
	, coalesce(p.gly_aim,0)
	, coalesce(p.gly_aim + 1.0,0)
	, coalesce(p.gly_max,0)
from mesdb.dbo.[ReadingTag] r
join mesdb.dbo.[ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
join mesdb.dbo.linetx i on i.lineid = r.LineId
join mesdb.dbo.[ProductCode] p on p.ProductCodeId = i.productcodeid
join mesdb.dbo.[Status] s on s.StatusId = i.statusid
where f.FieldName = 'csg_glyc_pct' and s.Code = 'RP'
 
insert into taglogs.dbo.[Limit]
select distinct t.tagid, i.stamp
	, coalesce(p.LF_Min,0)
	, coalesce(p.LF_LCL,0)
	, coalesce(p.LF_Aim,0)
	, coalesce(p.LF_UCL,0)
	, coalesce(p.LF_Max,0)
from mesdb.dbo.[ProductCode] p 
join mesdb.dbo.linetx i on p.ProductCodeId = i.productcodeid
join mesdb.dbo.[Line] l on l.lineid = i.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join mesdb.dbo.[Status] s on s.StatusId = i.statusid
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'layflat_mm_pv' and (d.name = 'Dry' or d.name = 'Tmp') and s.Code = 'RP'

insert into taglogs.dbo.[all]
select distinct t.tagid, s.code, i.stamp, 192
from mesdb.dbo.linetx i 
join mesdb.dbo.[Status] s on s.StatusId = i.StatusId 
join mesdb.dbo.line l on i.lineid = l.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'line_status' and i.stamp >= @archivecut

insert into taglogs.dbo.[past]
select distinct t.tagid, s.code, i.stamp
from mesdb.dbo.linetx i 
join mesdb.dbo.[Status] s on s.StatusId = i.StatusId 
join mesdb.dbo.line l on i.lineid = l.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'line_status' and i.stamp < @archivecut

