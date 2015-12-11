CREATE TABLE [dbo].[ProductCodeTx]
(
	[ProductCodeTxId] INT NOT NULL PRIMARY KEY, 
    [ProductCodeId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [UserId] VARCHAR(50) NOT NULL, 
    [Delta] VARCHAR(MAX) NOT NULL
)
