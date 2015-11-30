CREATE TABLE [dbo].[RecipeReading]
(
	[RecipeReadingId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SolutionRecipeId] INT NOT NULL, 
    [ReadingId] INT NOT NULL, 
    [LineColor] VARCHAR(20) NULL, 
    [Low] FLOAT NULL, 
    [High] FLOAT NULL, 
    CONSTRAINT [FK_RecipeReading_SolutionRecipe] FOREIGN KEY ([SolutionRecipeId]) REFERENCES [SolutionRecipe]([SolutionRecipeId]), 
    CONSTRAINT [FK_RecipeReading_Reading] FOREIGN KEY ([ReadingId]) REFERENCES [Reading]([ReadingId])
)
