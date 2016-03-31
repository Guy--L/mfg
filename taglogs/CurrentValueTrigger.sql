
CREATE trigger [dbo].[SyncValue2Current] 
ON [dbo].[All]
instead of INSERT AS
BEGIN
declare @pending table (
	[AllId] [int] IDENTITY(1,1),
	[TagId] [int],
	[Name] [varchar](64),
	[Value] [varchar](64),
	[Stamp] [datetime],
	[Quality] [int],
	[Retests] [int]
	)
declare @results table (
	[TagId] [int],
	[SetPoint] [char](2),
	[OldValue] [varchar](64),
	[SubMinute] [int]
	)

insert @pending 
select distinct i.tagid, '' name, i.value, i.stamp, i.quality, rank() over (partition by i.tagid order by i.stamp desc, i.allid desc) as retests
from inserted i

begin transaction
	merge [dbo].[Current] c
	using @pending p
	on c.TagId = p.TagId 
	when matched then 
		update set 
			c.Value = coalesce(p.Value, '0'), 
			c.Stamp = p.Stamp, 
			c.SubMinute = iif(p.Stamp >= dateadd(minute, 1, c.Stamp),1,c.SubMinute + 1)
	when not matched and p.retests = 1 then
		insert (tagid, name, stamp, value, subminute)
		values (p.tagid, p.name, p.stamp, p.value, 0)
	output 
		inserted.TagId, 
		left(reverse(deleted.Name),2),
		deleted.Value,
		inserted.SubMinute
		into @results;

	insert dbo.[All] (TagId, Value, Stamp, Quality)
	select p.TagId, coalesce(p.Value, '0'), p.Stamp, p.Quality
	from @pending p
	join @results r on r.TagId = p.TagId
	where r.SubMinute = 1

commit
END

