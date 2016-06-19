declare @julian decimal(5)

set @julian = (select top 1 inday from devusa.devusa.dcnitta.inltp856)

-- insert into lineoperation any recent changes in linestatus
; with swipe as (
	select s.inday, s.inunit, s.inline, s.inshft, s.stcode, s.intime, s.rscode, s.inprd,
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
select q.inday, q.inunit, q.inline, q.inshft, q.stcode, q.intime, q.rscode, q.inprd, q.stamp, q.LineId, 
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

