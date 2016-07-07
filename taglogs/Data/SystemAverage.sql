;with avgs as (
	select [sys], convert(varchar(64), avg(gly), 0) as value, g.stamp, 192 as quality from (
		select a.stamp, convert(float, a.value, 1) as gly, 'Sys'+convert(char(1), s.systemid, 1) as [sys]
		from [all] a
		join tag t on t.tagid = a.tagid
		join mesdb.dbo.[sample] s on s.sampleid = a.quality
		where t.name = 'csg_glyc_pct' and len(a.value) < 5 and a.stamp > '2016-06-30 01:00:00.000'
	) g
	group by stamp, [sys]
)
insert into [All] 
select t.TagId, v.value, v.stamp, v.quality
from avgs v 
join channel c on c.Name = v.[sys]
join device d on c.ChannelId = d.ChannelId
join tag t on t.DeviceId = d.DeviceId
where t.name = 'csg_glyc_pct'

