declare @new2insert table (
	tagid int,
	deviceid int
)

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
output inserted.tagid, inserted.deviceid into @new2insert
select d.deviceid, 'csg_moist_pct', 'x', 'Float', 0, 1 
from device d
join channel c on d.ChannelId = c.ChannelId
where d.name = 'Lab' and c.name like 'Sys%'

insert into [current]
select n.tagid, c.name+'.'+d.name+'.'+t.name,  0, dateadd(month, -2, getdate()), 1 
from @new2insert n 
join tag t on t.tagid = n.tagid
join [Device] d on d.DeviceId = n.deviceid
join [Channel] c on c.ChannelId = d.ChannelId