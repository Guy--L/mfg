select distinct q.systemid, q.[system], q.solutiontype, q.[datetime], q.completed from (SELECT b.[SolutionBatchId]
    ,b.[DateTime]
    ,b.[SolutionRecipeId]
    ,b.[OperatorId]
    ,b.[CoA]
    ,s.[SystemId]
    ,s.[System]
	,b.[Completed]
    ,r.[SolutionType]
    ,ROW_NUMBER() OVER(PARTITION BY b.[SystemId] ORDER BY b.[DateTime] DESC) AS Row
	,(select count(t.solutiontestid) from [dbo].[SolutionTest] t where t.SolutionBatchId = b.SolutionBatchId) as TestCount
FROM [dbo].[SolutionBatch] b
join [dbo].[SolutionRecipe] r on b.[SolutionRecipeId] = r.[SolutionRecipeId]
join [dbo].[System] s on b.[SystemId] = s.[Systemid]
where b.[DateTime] < '2016-01-20 11:01:03.747' and [system] not like 'gr%') q where q.row = 1 order by q.systemid

SELECT b.[SolutionBatchId]
    ,b.[DateTime]
    ,b.[SolutionRecipeId]
    ,s.[SystemId]
    ,s.[System]
	,b.[Completed]
    ,r.[SolutionType]
FROM [dbo].[SolutionBatch] b
join [dbo].[SolutionRecipe] r on b.[SolutionRecipeId] = r.[SolutionRecipeId]
join [dbo].[System] s on b.[SystemId] = s.[Systemid]
where b.[DateTime] < '2016-01-20 11:01:03.747' and (b.completed >= '2016-01-20 11:01:03.747' or b.completed is null) and [system] not like 'gr%'
order by b.[DateTime] desc

