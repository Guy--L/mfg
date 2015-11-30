update [dbo].[SolutionBatch] b
	set b.[Completed] = @0
	where b.[SolutionBatchId] = @1