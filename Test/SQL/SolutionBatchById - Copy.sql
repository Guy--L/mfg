            SELECT b.[SolutionBatchId]
                  ,b.[DateTime]
                  ,b.[SolutionRecipeId]
                  ,b.[OperatorId]
                  ,b.[CoA]
                  ,b.[SystemId]
                  ,r.[SolutionType]
              FROM [dbo].[SolutionBatch] b
              join [dbo].[SolutionRecipe] r on b.[SolutionRecipeId] = r.[SolutionRecipeId]
			  where b.[SolutionBatchId] = @0
