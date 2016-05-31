CREATE TABLE [dbo].[Review]
(
	[ReviewId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(50) NOT NULL, 
    [Schedule] VARCHAR(50) NULL, 
    [LastRun] DATETIME NULL, 
    [Type] VARCHAR(20) NULL, 
    [Path] VARCHAR(100) NULL, 
    [Template] VARCHAR(50) NULL 
)
