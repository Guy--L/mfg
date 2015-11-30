
create trigger [dbo].[SyncValue2Current] 
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
	[SetPoint] [char](2),
	[OldValue] [varchar](64),
	[SubMinute] [int]
	)

insert @pending 
select distinct tagid, value, stamp, quality
from inserted

begin transaction
	merge [dbo].[Current] c
	using @pending p
	on c.TagId = p.TagId
	when matched then 
		update set 
			c.Value = p.Value, 
			c.Stamp = p.Stamp, 
			c.SubMinute = iif(p.Stamp >= dateadd(minute, 1, c.Stamp),1,c.SubMinute + 1)
	output 
		inserted.TagId, 
		left(reverse(deleted.Name),2),
		deleted.Value,
		inserted.SubMinute
		into @results;

	-- for setpoints insert the prior value right before the new value for sanity
	insert dbo.[All] (TagId, Value, Stamp, Quality)
	select p.TagId, r.OldValue, dateadd(ms, -500, p.Stamp), p.Quality
	from @pending p
	join @results r on r.TagId = p.TagId
	where r.SubMinute = 1 and (r.SetPoint = 'ps' or r.SetPoint = 'ti')

	insert dbo.[All] (TagId, Value, Stamp, Quality)
	select p.TagId, p.Value, p.Stamp, p.Quality
	from @pending p
	join @results r on r.TagId = p.TagId
	where r.SubMinute = 1

	--update dbo.[All] set
	--	Value = convert(varchar(64), a.Value + (r.NewValue - a.Value)/r.SubMinute)
	--	from dbo.[All] a
	--	join @results r on r.TagId = a.TagId
	--	where not exists (
	--		select 1 from dbo.[All] b
	--		where b.TagId = a.TagId and b.Stamp > a.Stamp
	--	) and r.SubMinute > 1
commit
END

	--when not matched by target then
	--	insert into c (c.TagId, c.Name, c.Value, c.Stamp, c.SubMinute)
	--	select p.TagId, t.Name, p.Value, p.Stamp, 0
	--	from @pending p
	--	join Tag t on p.TagId = t.TagId
