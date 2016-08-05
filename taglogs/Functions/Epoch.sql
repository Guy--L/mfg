use taglogs
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
-- Create date: August 5, 2016
-- Description:	Convert from DateTime to Epoch (millseconds since 1970-1-1)
-- =============================================
ALTER FUNCTION [dbo].[Epoch](@Stamp datetime)
RETURNS bigint
AS
BEGIN
	RETURN cast(datediff(s, '1970-01-01 00:00:00', @Stamp) as bigint)*1000 + cast(datepart(ms, @Stamp) as bigint)
END
GO

