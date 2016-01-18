﻿CREATE TABLE [dbo].[Limit]
(
	[LimitId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TagId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [LoLo] FLOAT NOT NULL, 
    [Lo] FLOAT NOT NULL, 
    [Aim] FLOAT NOT NULL, 
    [Hi] FLOAT NOT NULL, 
    [HiHi] FLOAT NOT NULL
)