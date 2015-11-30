CREATE TABLE [dbo].[Device]
(
	[DeviceId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ChannelId] INT NOT NULL, 
    [Name] VARCHAR(64) NOT NULL, 
    [IPAddress] VARCHAR(32) NOT NULL, 
    [Model] VARCHAR(32) NOT NULL, 
    CONSTRAINT [FK_Device_ToChannel] FOREIGN KEY ([ChannelId]) REFERENCES [Channel]([ChannelId])
)
