CREATE TABLE [dbo].[SolutionBatch]
(
	[SolutionBatchId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DateTime] DATETIME NOT NULL, 
    [SolutionRecipeId] INT NOT NULL, 
    [OperatorId] INT NOT NULL, 
    [CoA] NCHAR(10) NOT NULL, 
    [SystemId] INT NOT NULL, 
    [Completed] DATETIME NULL, 
    CONSTRAINT [FK_SolutionBatch_System] FOREIGN KEY ([SystemId]) REFERENCES [System]([SystemId]), 
    CONSTRAINT [FK_SolutionBatch_SolutionRecipe] FOREIGN KEY ([SolutionRecipeId]) REFERENCES [SolutionRecipe]([SolutionRecipeId])
)
