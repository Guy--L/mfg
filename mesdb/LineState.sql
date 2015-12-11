CREATE TABLE [dbo].[LineState]
(
	[LineId] INT NOT NULL PRIMARY KEY, 
    [StatusId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL, 
    [SystemId] INT NOT NULL, 
    [LastChange] DATETIME NOT NULL
)
