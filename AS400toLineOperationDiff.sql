declare @recn integer 

set @recn = (select max(recordid) from lineoperation)

insert into lineoperation
select * from (
select s.*, 
	dbo.J2DateTime(inday, intime) as stamp,
	l.lineid,
	0 as productcodeid,
	ROW_NUMBER() over (order by inday, intime, inunit, inline) as recordid
from devusa.devusa.ncifiles#.indtp700 s
join unit u on s.inunit = u.Unit
join line l on l.unitid = u.unitid and s.inline = l.LineNumber
) g
where g.recordid > @recn

update n 
set n.productcodeid = coalesce(
	(select top 1 productcodeid
	from [plan]
	where stamp <= n.stamp and Code = n.inprd and lineid = n.lineid
	order by stamp desc),0
)
from lineoperation n
where n.recordid > @recn


--select * from lineoperation where recordid > 36475 order by stamp

--select m.*
--from lineoperation m
--left join lineoperation n on m.lineid = n.lineid and n.stamp > m.stamp
--where n.lineid is null
--order by lineid