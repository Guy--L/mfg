-- =============================================
-- Author:		Guy..Lister
-- Create date: August 5, 2016
-- Description:	Convert from DateTime to Epoch (millseconds since 1970-1-1)
-- =============================================
create FUNCTION [dbo].[Epoch](@Stamp datetime)
RETURNS bigint
AS
BEGIN
	RETURN cast(datediff(s, '1970-01-01 00:00:00', @Stamp) as bigint)*1000 + cast(datepart(ms, @Stamp) as bigint)
END
GO

