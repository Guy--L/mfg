CREATE TABLE [dbo].[CasingTest]
(
	[CasingTestId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LineId] INT NOT NULL, 
    [SystemId] INT NOT NULL, 
    [Reel] INT NULL, 
    [Delm] DECIMAL(18, 1) NULL, 
    [WetWt] DECIMAL(18, 1) NULL, 
    [DryWt] DECIMAL(18, 1) NULL, 
    [GlyWetWt] DECIMAL(18, 1) NULL, 
    [GlyArea] DECIMAL(18, 1) NULL, 
    [GlySTD] DECIMAL(18, 1) NULL, 
    [OilArea] DECIMAL(18, 1) NULL, 
    [Oil] DECIMAL(18, 1) NULL, 
    [DateTime] DATETIME NOT NULL, 
    [CasingGroupId] INT NULL, 
    [Feet] INT NULL, 
    CONSTRAINT [FK_CasingTest_CasingGroup] FOREIGN KEY ([CasingGroupId]) REFERENCES [CasingGroup]([CasingGroupId])
)
