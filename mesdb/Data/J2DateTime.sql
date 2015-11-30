-- ================================================
-- Template generated from Template Explorer using:
-- Create Scalar Function (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the function.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Guy..Lister
-- Create date: October 8, 2015
-- Description:	Convert from Julian Date and Time into normal datetime
-- =============================================
Alter FUNCTION J2DateTime(@JDate int, @JTime int)
RETURNS DateTime
AS
BEGIN
	RETURN dateadd(minute, @JTime%100, dateadd(hour, cast(@JTime/100 as int),
                    dateadd(day, @JDate%1000-1, dateadd(year, cast(@JDate/1000 as int), datetimefromparts(1990,1,1,0,0,0,0)))
                   ))

END
GO

