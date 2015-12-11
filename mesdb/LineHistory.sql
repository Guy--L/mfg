CREATE TABLE [dbo].[LineHistory]
(
	[LineHistoryId] int not null primary key identity,
	[LineId] INT NOT NULL, 
    [LineTankId] INT NULL, 
    [UnitId] INT NOT NULL, 
    [LineNumber] INT NOT NULL, 
    [SystemId] INT NULL, 
    [StatusId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [UserId] INT NOT NULL, 
    [Comment] VARCHAR(50) NULL, 
    [Taken] DATETIME NOT NULL
)
