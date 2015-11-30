USE [mesdb]
GO

SELECT t.[CasingTestId]
      ,t.[LineId]
	  ,(n.[UnitId]*10 + n.[LineNumber]) LineIndex
      ,s.[System]
      ,t.[ReelFt]
      ,t.[Delm]
      ,t.[WetWt]
      ,t.[DryWt]
      ,t.[GlyWetWt]
      ,t.[GlyArea]
      ,t.[GlySTD]
      ,t.[OilArea]
      ,t.[Oil]
      ,t.[DateTime]
  FROM [dbo].[CasingTest] t
  	join [dbo].[System] s on t.[SystemId] = s.[Systemid]
	join [dbo].[Line] n on t.[LineId] = n.[LineId]
	order by t.[DateTime] desc
GO