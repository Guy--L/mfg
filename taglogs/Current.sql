CREATE TABLE [dbo].[Current]
(
	[TagId] INT NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(64) NOT NULL, 
    [Value] VARCHAR(64) NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [SubMinute] INT NOT NULL
)
