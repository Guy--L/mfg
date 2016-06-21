select distinct m.lineid, u.Unit+convert(char, l.LineNumber, 1) as ln, p.ProductCode+' '+p.ProductSpec as code, s.Code from linetx m
left join linetx n on (n.lineid = m.lineid and n.stamp > m.stamp)
join line l on l.lineid = m.lineid
join unit u on u.unitid = l.unitid
join productcode p on p.ProductCodeId = m.ProductCodeId
join [status] s on s.StatusId = m.StatusId
where n.stamp is null
order by m.lineid

