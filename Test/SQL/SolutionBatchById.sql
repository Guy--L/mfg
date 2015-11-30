            SELECT b.[SolutionBatchId]
                  ,b.[DateTime]
                  ,b.[SolutionRecipeId]
                  ,b.[OperatorId]
                  ,b.[CoA]
                  ,b.[SystemId]
                  ,r.[SolutionType]
				  ,s.[System]
              FROM [dbo].[SolutionBatch] b
              join [dbo].[System] s on b.[SystemId] = s.[SystemId]
              join [dbo].[SolutionRecipe] r on b.[SolutionRecipeId] = r.[SolutionRecipeId]
			  where b.[SolutionBatchId] = @0
