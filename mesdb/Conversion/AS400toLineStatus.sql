delete from linestatus

insert into linestatus
select s.*, l.lineid, '?', '??', dateadd(year, 1, getdate()), 0 
from devusa.devusa.dcnitta.inltp856 s
join unit u on u.Unit = s.inunt
join line l on l.UnitId = u.UnitId and l.LineNumber = s.inlin
where inunt in ('A','B','C','D')

update s 
set s.[status] = o.stcode,
s.[reason] = o.[rscode],
s.[stamp] = o.[stamp]
from linestatus s 
join (
	select m.*
	from lineoperation m
	left join lineoperation n
	on (m.lineid = n.lineid and n.stamp > m.stamp)
	where n.lineid is null
	) o on o.lineid = s.lineid

update n 
set n.productcodeid = coalesce(
	(select top 1 productcodeid
	from [plan]
	where stamp <= n.stamp and Code = n.inprd and lineid = n.lineid
	order by stamp desc),0
)
from linestatus n
where n.productcodeid = 0

