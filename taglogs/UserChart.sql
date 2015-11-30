CREATE TABLE [dbo].[UserChart]
(
	[UserChartId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL, 
    [ChartId] INT NOT NULL, 
    [Shared] BIT NOT NULL
)
