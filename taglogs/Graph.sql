CREATE TABLE [dbo].[Graph]
(
	[GraphId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [GraphName] VARCHAR(50) NOT NULL, 
    [OwnerId] INT NOT NULL 
)
