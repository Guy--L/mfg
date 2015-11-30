insert 
into mesdb.dbo.ReadingTag
(ReadingFieldId, LineId, TagId) 
select r.ReadingFieldId, x.LineId, x.TagId from (
select q.LineId, t.Name, t.TagId
from taglogs.dbo.Tag t 
join taglogs.dbo.Device d on d.DeviceId = t.DeviceId
join taglogs.dbo.Channel c on d.ChannelId = c.ChannelId
join 
(select u.unit+cast(l.linenumber as char) as nm, l.lineid
from mesdb.dbo.Line l
join mesdb.dbo.unit u on l.UnitId = u.UnitId) q
on q.nm = c.Name
where d.Name = 'Lab'
--order by c.Name
) x
join mesdb.dbo.ReadingField r on r.FieldName = x.Name
