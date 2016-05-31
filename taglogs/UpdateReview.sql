CREATE PROCEDURE [dbo].[UpdateReview]
	@review varchar(50),
	@schedule varchar(50),
	@path varchar(100),
	@template varchar(50),
	@type varchar(20),
	@graphs varchar(MAX)
AS
	declare @reviewid int

    merge [review] r
    using (select [Name] = @review) n
    on n.[Name] = r.[Name]
    when not matched then
    insert ([Name], [Schedule], [Type], [Path], [Template])
    values (n.[Name], @schedule, @type, @path, @template)
	when matched then 
	update set [Schedule] = @schedule
		, [Type] = @type
		, [Path] = @path
		, [Template] = @template;

    select @reviewid = reviewid from [review] where name = @review

	update g
	set reviewid = 0
	from graph g
	left join dbo.SplitInts(@graphs, ',') t on t.Item = g.GraphId
	where reviewid = @reviewid and t.Item is null

	update g
	set reviewid = @reviewid
	from graph g
	join dbo.SplitInts(@graphs, ',') t on t.Item = g.GraphId

	select * from [review] where reviewid = @reviewid
RETURN 0
