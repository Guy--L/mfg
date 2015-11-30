select o.stamp, o.notes, u.[Identity], c.[Name] from operation o
join [user] u on o.userid = u.UserId
join [HMI] h on h.hmiid = o.HMIId
join [Channel] c on h.ChannelId = c.ChannelId
where o.stamp > '2015/08/19 23:30:00' and o.stamp < '2015/08/20 02:30:00'