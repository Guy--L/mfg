CREATE TABLE [dbo].[Booster]
(
	[BoosterId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SolutionBatchId] INT NOT NULL, 
    [DateTime] DATETIME NULL, 
    CONSTRAINT [FK_Booster_SolutionBatch] FOREIGN KEY ([SolutionBatchId]) REFERENCES [SolutionBatch]([SolutionBatchId])
)
