CREATE TABLE [dbo].[Chart]
(
	[ChartId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ChartName] VARCHAR(50) NOT NULL, 
    [OwnerId] INT NOT NULL 
)
