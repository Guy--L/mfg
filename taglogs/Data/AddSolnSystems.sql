declare @newinsert table (
	id int
)

declare @new2insert table (
	id int,
	deviceid int
)

insert into channel
select 'Sys'+[system], null from mesdb.dbo.[system] 
where [status] = 'Good' and isnumeric([system]) = 1

insert into device
output inserted.deviceid into @newinsert
select channelid, 'Lab', '127.0.0.1', 'DL-260'
from channel
where name like 'Sys%'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
output inserted.tagid, inserted.deviceid into @new2insert
select d.id, 'csg_glyc_pct', 'x', 'Float', 0, 1 
from @newinsert d

insert into [current]
select t.id, c.name+'.'+d.name+'.'+t.name,  0, dateadd(month, -2, getdate()), 1 
from @new2insert t 
join [Device] d on d.DeviceId = t.deviceid
join [Channel] c on c.ChannelId = d.ChannelId