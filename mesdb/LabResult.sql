USE [mesdb]
GO

/****** Object: View [dbo].[LabResult] Script Date: 11/16/2015 6:08:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[LabResult]
	AS 
SELECT 
cast(coalesce(iif(r.R1=0, 1, r.R1), 1) as float) r1,
cast(coalesce(iif(r.R2=0, 1, r.R2), 1) as float) r2,
cast(coalesce(iif(r.R3=0, 1, r.R3), 1) as float) r3,
cast(coalesce(iif(r.R4=0, 1, r.R4), 1) as float) r4,
cast(coalesce(iif(r.R5=0, 1, r.R5), 1) as float) r5,
r.Stamp,
s.Completed,
s.LineId,
s.Reading3 as OilPct,
s.SampleId
FROM [Sample] s
join [Reading] r on s.SampleId = r.SampleId
