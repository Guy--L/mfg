﻿CREATE TABLE [dbo].[Group]
(
	[GroupId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(50) NOT NULL, 
    [UserId] INT NOT NULL
)
