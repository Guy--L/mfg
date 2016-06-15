insert into lineoperation
select s.*, 
	dbo.J2DateTime(inday, intime) as stamp,
	l.lineid,
	0 as productcodeid,
	ROW_NUMBER() over (order by inday, intime, inunit, inline) as recordid
from devusa.devusa.ncifiles#.indtp700 s
join unit u on s.inunit = u.Unit
join line l on l.unitid = u.unitid and s.inline = l.LineNumber

update n 
set n.productcodeid = coalesce(
	(select top 1 productcodeid
	from [plan]
	where stamp <= n.stamp and Code = n.inprd 
	order by stamp desc),0
)
from lineoperation n
where n.INDAY > 23000 and n.productcodeid = 0

update n 
set n.productcodeid = coalesce(
	(select top 1 productcodeid
	from [plan]
	where stamp <= n.stamp and Code = n.inprd 
	order by stamp desc),0
)
from lineoperation n
where n.INDAY > 23000 and n.productcodeid = 0
