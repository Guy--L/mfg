CREATE TABLE [dbo].[Plan]
(
	[PlanId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Stamp] DATETIME NOT NULL, 
    [LineId] INT NOT NULL, 
    [ProductCodeId] INT NULL, 
    [Code] VARCHAR(10) NOT NULL, 
    [Spec] VARCHAR(10) NOT NULL, 
    [ExtruderId] INT NOT NULL, 
    [Solution] VARCHAR(10) NULL, 
    [SystemId] INT NULL, 
    [SolutionRecipeId] INT NULL, 
    [ConversionStatus] CHAR NULL, 
    [Comment] VARCHAR(100) NULL
)
