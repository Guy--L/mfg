CREATE PROCEDURE [dbo].[UpdatePlot]
	@login varchar(50),
	@identity varchar(50),
	@graph varchar(50),
	@tags varchar(MAX)
AS
	declare @graphid int
    declare @tagname table(
        [Id] int,
        [Device] [varchar](64),
        [Name] [varchar](64)
    )
	declare @userid as int

    ;with newtag as (
		select t.TagId, d.Name Device, t.Name, row_number() over(partition by d.Name+t.Name order by t.TagId) as rnk 
		from Tag t
		join Device d on d.DeviceId = t.DeviceId
		join dbo.SplitInts(@tags, ',') s on s.Item = t.TagId
	)
	insert into @tagname
	select q.TagId, q.Device, q.Name 
	from newtag q
	where rnk = 1

    Merge [user] u
    using (select [Login] = @login) s 
    ON s.[Login] = u.[Login]
    WHEN NOT matched THEN 
    INSERT ([Login], [Identity]) VALUES (s.[Login], @identity);

    select @userid = userid from [user] where [Login] = @login

    merge [graph] g
    using (select [Name] = @graph) n
    on n.[Name] = g.[GraphName] and g.[UserId] = @userid
    when not matched then 
    insert ([GraphName], [UserId], [Shared])
    values (n.[Name], @userid, 0);

    select @graphid = graphid from [graph] where graphname = @graph and userid = @userid

    delete p
    from [plot] p
    join [tag] t on t.tagid = p.tagid
    join [device] d on d.deviceid = t.deviceid
    left join @tagname n on t.name = n.name and d.name = n.device
    where n.name is null and p.graphid = @graphid

    insert [plot]
    select @graphid, t.id, 0, t.Device + '.' + t.name, 1, -1, -1
    from @tagname t
    left join [plot] p on t.id = p.tagid and p.graphid = @graphid
    where p.plotid is null

RETURN 0
