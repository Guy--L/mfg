select n.rscode, count(*) as frq, 
(select count(*) from lineoperation where stcode = 'U' and rscode = n.rscode and year(stamp) > 2014) as [up],
(select count(*) from lineoperation where stcode = 'D' and rscode = n.rscode and year(stamp) > 2014) as [down]
from lineoperation n
where year(stamp) > 2014
group by rscode
order by frq desc
