ALTER trigger [dbo].[SyncValue2Current] 
ON [dbo].[All]
instead of INSERT AS
BEGIN
declare @pending table (
	[AllId] [int] IDENTITY(1,1),
	[TagId] [int],
	[Value] [varchar](64),
	[Stamp] [datetime],
	[Quality] [int]
	)
declare @results table (
	[TagId] [int],
	[OldValue] [varchar](64),
	[SubMinute] [int]
	)

insert @pending 
select distinct tagid, value, stamp, quality
from inserted

begin transaction
	insert into [current] 
	select p.tagid, c.name+'.'+d.name+'.'+t.name, 0, getdate(), 1
	from @pending p
	join tag t on p.tagid = t.tagid
	join device d on t.DeviceId = d.DeviceId
	join channel c on c.ChannelId = d.ChannelId
	left join [current] n on n.tagid = p.tagid
	where n.tagid is null

	update c set 
		c.Value = coalesce(p.Value, '0'), 
		c.Stamp = p.Stamp, 
		c.SubMinute = iif(p.Stamp >= dateadd(minute, 1, c.Stamp),1,c.SubMinute + 1)
	output inserted.TagId, deleted.Value, inserted.SubMinute into @results
	from [Current] c
	join @pending p on c.tagid = p.tagid
	left join @pending q on (p.tagid = q.tagid and q.stamp > p.stamp)
	where q.tagid is null

	insert dbo.[All] (TagId, Value, Stamp, Quality)
	select p.TagId, coalesce(p.Value, '0'), p.Stamp, p.Quality
	from @pending p
	join @results r on r.TagId = p.TagId
	where r.SubMinute = 1

commit
END

