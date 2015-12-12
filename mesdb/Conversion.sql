﻿CREATE TABLE [dbo].[Conversion]
(
	[ConversionId] INT NOT NULL PRIMARY KEY, 
    [LineId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL, 
    [SystemId] INT NOT NULL, 
	[SolutionRecipeId] int not null,
    [ExtruderId] INT NOT NULL, 
    [Scheduled] DATETIME NOT NULL, 
    [Started] DATETIME NULL, 
    [Completed] DATETIME NOT NULL, 
    [FinishFootage] INT NOT NULL, 
    [Exempt] BIT NOT NULL, 
    [ExemptCode] VARCHAR(20) NULL, 
    [Note] VARCHAR(50) NULL
)