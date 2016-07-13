;with cut as (
	select lineid, stamp, productcodeid, statusid, endstamp from (
		select distinct lineid, stamp, productcodeid, statusid, 
			lead(stamp, 1, getdate()) over (partition by lineid order by stamp) as endstamp
		from linetx
	) a
	where a.stamp < '2016-07-06 16:00:00.000'
)
select n.lineid, n.stamp, n.endstamp,
u.Unit+convert(char, l.LineNumber, 1) as ln, p.ProductCode+' '+p.ProductSpec as code, s.Code 
from cut n
left join cut m on n.lineid = m.lineid and m.stamp > n.stamp
join line l on l.lineid = n.lineid
join unit u on u.unitid = l.unitid
join productcode p on p.ProductCodeId = n.ProductCodeId
join [status] s on s.StatusId = n.StatusId
where m.stamp is null and n.productcodeid = 928 and s.Code = 'RP'
