;with cut as (
select distinct lineid, stamp, productcodeid, statusid
from linetx
where stamp < '2016-05-08 16:00:00.000'
) 
select m.lineid, m.stamp,
u.Unit+convert(char, l.LineNumber, 1) as ln, p.ProductCode+' '+p.ProductSpec as code, s.Code 
from cut m 
left join cut n on (n.lineid = m.lineid and n.stamp > m.stamp)
join line l on l.lineid = m.lineid
join unit u on u.unitid = l.unitid
join productcode p on p.ProductCodeId = m.ProductCodeId
join [status] s on s.StatusId = m.StatusId
where n.stamp is null 
order by m.lineid
