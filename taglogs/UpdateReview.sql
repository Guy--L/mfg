CREATE PROCEDURE [dbo].[UpdateReview]
	@review varchar(50),
	@schedule varchar(50),
	@graphs varchar(MAX)
AS
	declare @reviewid int

    merge [review] r
    using (select [Name] = @review) n
    on n.[Name] = r.[Name]
    when not matched then
    insert ([Name], [Schedule])
    values (n.[Name], @schedule)
	when matched then
	update set [Schedule] = @schedule;

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

RETURN 0
