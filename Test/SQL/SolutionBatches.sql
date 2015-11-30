SELECT b.[SolutionBatchId]
        ,b.[DateTime]
        ,b.[SolutionRecipeId]
        ,b.[OperatorId]
        ,b.[CoA]
        ,s.[System]
		,b.[Completed]
        ,r.[SolutionType]
        ,ROW_NUMBER() OVER(PARTITION BY b.[SystemId] ORDER BY b.[DateTime] DESC) AS Row
		,(select count(t.solutiontestid) from [dbo].[SolutionTest] t where t.SolutionBatchId = b.SolutionBatchId) as TestCount
    FROM [dbo].[SolutionBatch] b
    join [dbo].[SolutionRecipe] r on b.[SolutionRecipeId] = r.[SolutionRecipeId]
	join [dbo].[System] s on b.[SystemId] = s.[Systemid]
    order by s.[System] asc
