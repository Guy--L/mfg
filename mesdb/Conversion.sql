CREATE TABLE [dbo].[Conversion]
(
	[ConversionId] INT NOT NULL PRIMARY KEY IDENTITY, 
	[LineTxId] int not null,
	[SolutionRecipeId] int not null,
    [ExtruderId] INT NOT NULL, 
    [FinishFootage] INT NOT NULL, 
    [EndStateId] INT NOT NULL, 
    [ConversionState] INT NOT NULL, 
)
