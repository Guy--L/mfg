select a.Cnt, c.Name, d.Name, t.* from Tag t
left join (
select count(*) Cnt, TagId
from [All]
group by TagId
) a on t.TagId = a.TagId 
join Device d on t.DeviceId = d.DeviceId
join Channel c on d.ChannelId = c.ChannelId
where t.Name like '%layflat%'
order by c.Name, d.Name


