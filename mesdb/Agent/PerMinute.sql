declare @julian decimal(5)

set @julian = (select top 1 inday from devusa.devusa.dcnitta.inltp856)

-- insert into lineoperation any recent changes in linestatus
; with swipe as (
	select distinct s.inday, s.inunit, s.inline, s.inshft, s.stcode, s.intime, 
		case 
		when s.stcode = 'U' then 'RP' 
		when s.rscode = 'D' then 'DN'
		when rtrim(s.rscode) = '' then 'DN'
		when s.rscode = 'RM' or s.rscode = 'PF' then 'PM'
		else s.rscode
		end as rscode, 
	s.inprd,
	dbo.J2DateTime(inday, intime) as stamp,
	l.lineid,
	0 as productcodeid,
	0 as recordid,
	row_number() over (partition by s.inunit, s.inline order by s.inday desc, s.intime desc) as latest
	from devusa.devusa.ncifiles#.indtp700 s
	join unit u on s.inunit = u.Unit
	join line l on l.unitid = u.unitid and s.inline = l.LineNumber
	where s.inday >= @julian+19999
)
insert into lineoperation
select distinct q.inday, q.inunit, q.inline, q.inshft, q.stcode, q.intime, 
q.rscode,
q.inprd, q.stamp, q.LineId, 
coalesce(
	(select top 1 productcodeid
	from [plan]
	where stamp <= q.stamp and Code = q.inprd and lineid = q.lineid
	order by stamp desc),0
), q.recordid 
from swipe q
left join linestatus s on s.inunt = q.inunit and s.inlin = q.inline and s.[status] = q.stcode
where q.latest = 1 and s.inunt is null

-- Update line status from AS400 to SQL Server
--  when there is a difference

update s set s.inprd = a.inprd,
s.insid = a.insid,
s.[status] = o.stcode,
s.[reason] = o.[rscode],
s.[stamp] = o.[stamp],
s.[ProductCodeId] = o.ProductCodeId
from linestatus s
join devusa.devusa.dcnitta.inltp856 a on s.inunt = a.inunt and s.inlin = a.inlin
join (
	select m.*
	from lineoperation m
	left join lineoperation n
	on (m.lineid = n.lineid and n.stamp > m.stamp)
	where n.lineid is null
	) o on o.lineid = s.lineid
where s.inprd != a.inprd or s.insid != a.insid or s.[status] != o.stcode

--set up for product change
declare @newinsert table (
	ltxid int,
	stamp datetime,
	lineid int,
	statusid int,
	productcodeid int,
	planid int
)

insert into linetx
output inserted.LineTxId, inserted.stamp, inserted.lineid, inserted.statusid, inserted.productcodeid, 
	(select top 1 comment
		from [plan]
		where lineid = inserted.lineid
		and productcodeid = inserted.ProductCodeId
		and stamp <= inserted.stamp 
		order by stamp desc) as planid
 into @newinsert
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
	where n.RecordId = 0

update n
set n.recordid = i.ltxid
from lineoperation n
join @newinsert i on i.lineid = n.lineid and i.stamp = n.stamp and i.productcodeid = n.productcodeid

use [taglogs]

declare @archivecut datetime
select @archivecut = min(stamp) from [all]

insert into [all]
select t.tagid, p.productcode+' '+p.productspec as product, i.stamp, 192 as quality
from @newinsert i
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
from @newinsert i
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
join @newinsert i on i.lineid = r.LineId
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
join @newinsert i on i.lineid = r.LineId
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
join @newinsert i on p.ProductCodeId = i.productcodeid
join mesdb.dbo.[Line] l on l.lineid = i.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join mesdb.dbo.[Status] s on s.StatusId = i.statusid
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'layflat_mm_pv' and (d.name = 'Dry' or d.name = 'Tmp') and s.Code = 'RP'

insert into taglogs.dbo.[all]
select distinct t.tagid, s.code, i.stamp, 192
from @newinsert i 
join mesdb.dbo.[Status] s on s.StatusId = i.StatusId 
join mesdb.dbo.line l on i.lineid = l.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'line_status' and i.stamp >= @archivecut

insert into taglogs.dbo.[past]
select distinct t.tagid, s.code, i.stamp
from @newinsert i 
join mesdb.dbo.[Status] s on s.StatusId = i.StatusId 
join mesdb.dbo.line l on i.lineid = l.lineid
join mesdb.dbo.[Unit] u on u.unitid = l.UnitId
join taglogs.dbo.[Channel] c on (u.unit+cast(l.linenumber as char)) = c.name
join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
where t.name = 'line_status' and i.stamp < @archivecut

update n
set n.linetankid = i.linetankid,
n.systemid = i.systemid,
n.statusid = i.statusid,
n.productcodeid = i.productcodeid,
n.stamp = i.stamp,
n.personid = i.personid,
n.conversionid = i.planid
from line n
join @newinsert i on i.lineid = n.lineid
