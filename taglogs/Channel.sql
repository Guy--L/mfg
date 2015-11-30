CREATE TABLE [dbo].[Channel]
(
	[ChannelId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(64) NOT NULL, 
    [PlantId] INT NULL 
)
