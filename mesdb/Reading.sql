CREATE TABLE [dbo].[Reading]
(
	[ReadingId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LineId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [R1] INT NULL, 
    [R2] INT NULL, 
    [R3] INT NULL, 
    [R4] INT NULL, 
	[R5] INT NULL,
    [Average] INT NULL, 
    [ParameterId] INT NOT NULL, 
    [Operator] VARCHAR(20) NOT NULL, 
    [EditCount] INT NOT NULL, 
    [Scheduled] DATETIME NOT NULL, 
    [SampleId] INT NOT NULL
)
