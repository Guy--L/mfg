-- =============================================
-- Author:		Guy..Lister
-- Create date: January 26, 2016
-- Description:	Return smallest difference from now in seconds
-- =============================================
CREATE FUNCTION SinceNow(@Started datetime, @Completed datetime)
RETURNS int
AS
BEGIN
	declare @sinceStart int;
	declare @sinceComplete int;

	set @sinceStart = case when datediff(d, @Started, getdate()) >= 0 then datediff(s, @Started, getdate()) else 100000000 end
	set @sinceComplete = case when datediff(d, @Completed, getdate()) >= 0 then datediff(s, @Completed, getdate()) else 100000000 end

	return case when @sinceStart > @sinceComplete then @sinceComplete else @sinceStart end

END