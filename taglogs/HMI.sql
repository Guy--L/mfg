CREATE TABLE [dbo].[HMI]
(
	[HMIId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ChannelId] INT NOT NULL, 
    [ChartId] INT NOT NULL, 
    [RequestPending] INT NOT NULL, 
    [RequestComplete] BIT NOT NULL, 
    [Error] BIT NOT NULL, 
    [Expires] DATETIME NOT NULL, 
    CONSTRAINT [FK_HMI_ToChannel] FOREIGN KEY ([ChannelId]) REFERENCES [Channel]([ChannelId]), 
    CONSTRAINT [FK_HMI_ToChart] FOREIGN KEY ([ChartId]) REFERENCES [Chart]([ChartId])
)
