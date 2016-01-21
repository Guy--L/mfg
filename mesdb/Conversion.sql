CREATE TABLE [dbo].[Conversion]
(
	[ConversionId] INT NOT NULL PRIMARY KEY identity, 
    [LineId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL, 
    [SystemId] INT NOT NULL, 
	[SolutionRecipeId] int not null,
    [ExtruderId] INT NOT NULL, 
    [Scheduled] DATETIME NOT NULL, 
    [StatusId] INT NOT NULL default 1,
    [Started] DATETIME NULL, 
    [Completed] DATETIME NOT NULL, 
    [FinishFootage] INT NOT NULL, 
    [Exempt] BIT NOT NULL, 
    [ExemptId] INT NULL, 
    [Note] VARCHAR(50) NULL 
)
