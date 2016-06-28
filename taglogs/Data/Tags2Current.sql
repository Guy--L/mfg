	update c set 
		c.Value = coalesce(p.Value, '0'), 
		c.Stamp = p.Stamp, 
		c.SubMinute = iif(p.Stamp >= dateadd(minute, 1, c.Stamp),1,c.SubMinute + 1)
	from [Current] c
	join [All] p on c.tagid = p.tagid
	join Tag t on p.tagid = t.tagid
	left join [All] q on (p.tagid = q.tagid and q.stamp > p.stamp)
	where t.name in ('csg_glyc_pct', 'csg_moist_pct') and q.tagid is null
