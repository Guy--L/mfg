﻿CREATE TABLE [dbo].[Role]
(
	[RoleId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Role] VARCHAR(50) NOT NULL, 
    [ADGroup] VARCHAR(50) NOT NULL
)
