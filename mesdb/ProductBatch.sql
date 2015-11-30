CREATE TABLE [dbo].[ProductBatch]
(
	[ProductBatchId] INT NOT NULL PRIMARY KEY, 
    [DateTime] DATETIME NOT NULL, 
    [OperatorId] INT NOT NULL, 
    [SolutionBatchId] INT NOT NULL, 
    [GelBatchId] INT NOT NULL, 
    [ProductRecipeId] INT NOT NULL, 
    [Status] CHAR NOT NULL, 
    [EcrId] INT NOT NULL, 
    CONSTRAINT [FK_ProductBatch_GelBatch] FOREIGN KEY ([GelBatchId]) REFERENCES [GelBatch]([GelBatchId]), 
    CONSTRAINT [FK_ProductBatch_SolutionBatch] FOREIGN KEY ([SolutionBatchId]) REFERENCES [SolutionBatch]([SolutionBatchId]), 
    CONSTRAINT [FK_ProductBatch_ProductRecipe] FOREIGN KEY ([ProductRecipeId]) REFERENCES [ProductRecipe]([ProductRecipeId])
)
