CREATE TABLE [dbo].[Status]
(
	[StatusId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Code] VARCHAR(5) NOT NULL, 
    [Description] VARCHAR(30) NOT NULL, 
    [Icon] VARCHAR(20) NOT NULL, 
    [Color] VARCHAR(20) NOT NULL
)
