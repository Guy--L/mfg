update n
set n.linetankid = i.linetankid,
n.systemid = i.systemid,
n.statusid = i.statusid,
n.productcodeid = i.productcodeid,
n.stamp = i.stamp,
n.personid = i.personid,
n.conversionid = (select top 1 planid
		from [plan]
		where lineid = i.lineid
		and productcodeid = i.ProductCodeId
		and stamp <= i.stamp 
		order by stamp desc)
from line n
join linetx i on n.lineid = i.lineid
left join linetx j on (i.lineid = j.lineid and j.stamp > i.stamp)
where j.stamp is null
