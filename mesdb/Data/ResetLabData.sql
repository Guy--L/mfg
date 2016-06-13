declare @year int
set @year = 2015

use taglogs

delete from limit where year(stamp) >= @year

delete a from [all] a
join tag t on a.tagid = t.tagid
where t.name in ('product_code', 'csg_moist_pct', 'csg_glyc_pct', 'line_status')

delete p from [past] p
join tag t on p.tagid = t.tagid
where t.name in ('product_code', 'csg_moist_pct', 'csg_glyc_pct', 'line_status')
and year(p.stamp) >= @year

use mesdb

delete from linetx where year(stamp) >= @year
DBCC CHECKIDENT ('[linetx]', RESEED, 0)


