CREATE TABLE [dbo].[LineTx]
(
	[LineTxId] INT NOT NULL PRIMARY KEY, 
    [LineId] INT NOT NULL, 
    [UserId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [Comment] VARCHAR(200) NULL,
    [LineTankId] INT NULL, 
    [UnitId] INT NOT NULL, 
    [LineNumber] INT NOT NULL, 
    [SystemId] INT NULL, 
    [StatusId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL
)
