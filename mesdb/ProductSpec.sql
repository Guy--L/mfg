CREATE TABLE [dbo].[ProductSpec]
(
	[ProductSpecId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ProductCodeId] INT NOT NULL, 
    [DateTime] DATETIME NOT NULL, 
    [SolutionRecipeId] INT NOT NULL, 
    CONSTRAINT [FK_ProductSpec_ProductCode] FOREIGN KEY ([ProductCodeId]) REFERENCES [ProductCode]([ProductCodeId]), 
    CONSTRAINT [FK_ProductSpec_SolutionRecipe] FOREIGN KEY ([SolutionRecipeId]) REFERENCES [SolutionRecipe]([SolutionRecipeId])
)
