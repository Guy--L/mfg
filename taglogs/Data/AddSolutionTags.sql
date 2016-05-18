
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS1')
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS2')
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS4')
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS5')
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS6')
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS7')
INSERT INTO [dbo].[Channel] ([Name]) VALUES ('SS8')

insert into device (channelid, name, ipaddress, model)
select channelid, 'Lab', '127.0.0.1', 'DL-260' from channel c
where c.Name like 'SS%'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'csg_glyc_pct', 'x', 'Float', 0, 1 
from device d
join channel c on d.ChannelId = c.ChannelId
where d.name = 'Lab' and c.Name like 'SS%'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'csg_moist_pct', 'x', 'Float', 0, 1 
from device d
join channel c on d.ChannelId = c.ChannelId
where d.name = 'Lab' and c.Name like 'SS%'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'csg_oil_pct', 'x', 'Float', 0, 1 
from device d
join channel c on d.ChannelId = c.ChannelId
where d.name = 'Lab' and c.Name like 'SS%'
