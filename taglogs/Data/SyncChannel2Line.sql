-- read lines and units on mesdb and make channels on tags
-- where they don't already exist  A1...D8

insert into channel (name)
select nm from 
(select u.unit+cast(l.linenumber as char) as nm
from mesdb.dbo.Line l
join mesdb.dbo.unit u on l.UnitId = u.UnitId) q
left join (select name from taglogs.dbo.channel) x on x.name = q.nm
where x.name is null
