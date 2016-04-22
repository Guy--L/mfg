CREATE TABLE [dbo].[UserGraph]
(
	[UserGraphId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL, 
    [GraphId] INT NOT NULL, 
    [Shared] BIT NOT NULL
)
