;with cte as (
	SELECT t1.tagid, t.name, t1.value, t1.stamp, rank() over (partition by t1.tagid order by t1.stamp desc, t1.allid desc) as retests
	FROM [ALL] t1
	join tag t on t1.tagid = t.tagid
	WHERE t.name like 'csg_%'
)
update c
set c.value = n.value,
c.stamp = n.stamp
from [current] c
join cte n on n.tagid = c.tagid
where n.retests = 1 
