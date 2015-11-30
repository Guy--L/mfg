CREATE TABLE [dbo].[Tag]
(
	[TagId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DeviceId] INT NOT NULL, 
    [Name] VARCHAR(64) NOT NULL, 
    [Address] VARCHAR(16) NOT NULL, 
    [DataType] VARCHAR(16) NOT NULL, 
    [IsLogged] BIT NOT NULL, 
    [IsArchived] BIT NOT NULL, 
    [RelatedTagId] INT NULL, 
    CONSTRAINT [FK_Tag_ToDevice] FOREIGN KEY ([DeviceId]) REFERENCES [Device]([DeviceId]) 
)
