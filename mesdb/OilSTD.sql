CREATE TABLE [dbo].[OilSTD]
(
	[OilSTDId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DateTime] DATETIME NULL, 
    [Concentration] INT NULL, 
    [Area] FLOAT NOT NULL, 
    [CasingGroupId] INT NULL, 
    CONSTRAINT [FK_OilSTD_CasingGroup] FOREIGN KEY ([CasingGroupId]) REFERENCES [CasingGroup]([CasingGroupId])
)
