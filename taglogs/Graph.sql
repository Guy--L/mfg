CREATE TABLE [dbo].[Graph]
(
	[GraphId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [GraphName] VARCHAR(50) NOT NULL, 
    [UserId] INT NULL, 
    [Shared] BIT NOT NULL, 
    [ReviewId] INT NULL 
)
