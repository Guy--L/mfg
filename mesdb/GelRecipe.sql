CREATE TABLE [dbo].[GelRecipe]
(
	[GelRecipeId] INT NOT NULL PRIMARY KEY, 
    [CelluloseId] INT NOT NULL, 
    [WaterId] INT NOT NULL, 
    [AcidId] INT NOT NULL, 
    [Ice] INT NOT NULL, 
    [GelType] INT NOT NULL, 
    [MicroCutCount] INT NOT NULL, 
    [MicroMix] INT NOT NULL, 
    [HomoPass1] INT NOT NULL, 
    [HomoPass2] INT NOT NULL, 
    [AgeTime] INT NOT NULL, 
    CONSTRAINT [FK_GelRecipe_Cellulose] FOREIGN KEY ([CelluloseId]) REFERENCES [Cellulose]([CelluloseId]), 
    CONSTRAINT [FK_GelRecipe_Water] FOREIGN KEY ([WaterId]) REFERENCES [Water]([WaterId]), 
    CONSTRAINT [FK_GelRecipe_Acid] FOREIGN KEY ([AcidId]) REFERENCES [Acid]([AcidId])
)
