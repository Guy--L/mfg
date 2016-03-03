﻿CREATE TABLE [dbo].[LineTx]
(
	[LineTxId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LineId] INT NOT NULL, 
    [PersonId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [Comment] VARCHAR(200) NULL,
    [LineTankId] INT NULL, 
    [UnitId] INT NOT NULL, 
    [LineNumber] INT NOT NULL, 
    [SystemId] INT NULL, 
    [StatusId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL
)
