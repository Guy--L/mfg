CREATE TABLE [dbo].[GelBatch]
(
	[GelBatchId] INT NOT NULL PRIMARY KEY, 
    [DateTime] DATETIME NOT NULL, 
    [GelRecipeId] INT NOT NULL, 
    [OperatorId] INT NOT NULL, 
    CONSTRAINT [FK_GelBatch_GelRecipe] FOREIGN KEY ([GelRecipeId]) REFERENCES [GelRecipe]([GelRecipeId])
)
