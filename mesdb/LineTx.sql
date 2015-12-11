CREATE TABLE [dbo].[LineTx]
(
	[LineTxId] INT NOT NULL PRIMARY KEY, 
    [LineId] INT NOT NULL, 
    [UserId] VARCHAR(50) NOT NULL, 
    [Delta] VARCHAR(MAX) NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [Comment] VARCHAR(200) NULL
)
