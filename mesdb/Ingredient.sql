CREATE TABLE [dbo].[Ingredient]
(
	[IngredientId] INT NOT NULL PRIMARY KEY Identity, 
    [SolutionRecipeId] INT NULL, 
    [ProductSpecId] INT NULL, 
	[IsBooster] bit not null,
    [DateTime] DATETIME NOT NULL, 
    [RawMaterialId] INT NOT NULL, 
	[RawMaterialType] nchar(10) null,
    [Percentage] DECIMAL(18, 4) NOT NULL, 
    [High] DECIMAL(18, 4) NOT NULL, 
    [Low] DECIMAL(18, 4) NOT NULL, 
    CONSTRAINT [FK_Ingredient_SolutionRecipe] FOREIGN KEY ([SolutionRecipeId]) REFERENCES [SolutionRecipe]([SolutionRecipeId]), 
    CONSTRAINT [FK_Ingredient_ProductSpec] FOREIGN KEY ([ProductSpecId]) REFERENCES [ProductSpec]([ProductSpecId]), 
    CONSTRAINT [FK_Ingredient_RawMaterial] FOREIGN KEY ([RawMaterialId]) REFERENCES [RawMaterial]([RawMaterialId]) 
)
