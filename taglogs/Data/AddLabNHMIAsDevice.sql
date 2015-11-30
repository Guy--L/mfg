insert into device (channelid, name, ipaddress)
select channelid, 'Lab', '127.0.0.1' from channel 

insert into device (channelid, name, ipaddress)
select channelid, 'HMI', '127.0.0.1' from channel