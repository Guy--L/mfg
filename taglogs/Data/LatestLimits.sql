SELECT c.name+'.'+d.name+'.'+t.name name, t1.*
FROM limit t1
join tag t on t1.tagid = t.tagid
join device d on t.deviceid = d.DeviceId
join channel c on d.ChannelId = c.ChannelId
LEFT OUTER JOIN limit t2 ON (t1.tagid = t2.tagid AND t1.stamp < t2.stamp)
where t2.tagid is null
order by name
