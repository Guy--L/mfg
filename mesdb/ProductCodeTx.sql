CREATE TABLE [dbo].[ProductCodeTx]
(
	[ProductCodeTxId] INT NOT NULL PRIMARY KEY, 
    [ProductCodeId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [PersonId] int NOT NULL, 
    [Delta] VARCHAR(MAX) NOT NULL
)
