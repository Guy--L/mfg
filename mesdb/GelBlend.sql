CREATE TABLE [dbo].[GelBlend]
(
	[GelBlendId] INT NOT NULL PRIMARY KEY, 
    [GrindBatchId] INT NOT NULL, 
    [GelRecipeId] INT NOT NULL, 
    [Percent] INT NOT NULL, 
    [HideTypeId] INT NOT NULL, 
    CONSTRAINT [FK_GelBlend_GrindBatch] FOREIGN KEY ([GrindBatchId]) REFERENCES [GrindBatch]([GrindBatchId]), 
    CONSTRAINT [FK_GelBlend_GelRecipe] FOREIGN KEY ([GelRecipeId]) REFERENCES [GelRecipe]([GelRecipeId]), 
    CONSTRAINT [FK_GelBlend_HideType] FOREIGN KEY ([HideTypeId]) REFERENCES [HideType]([HideTypeId])
)
