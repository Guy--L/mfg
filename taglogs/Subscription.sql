CREATE TABLE [dbo].[Subscription]
(
	[SubscriptionId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TagId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [Value] VARCHAR(64) NOT NULL
)
