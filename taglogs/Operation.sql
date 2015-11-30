CREATE TABLE [dbo].[Operation]
(
	[OperationId] INT NOT NULL PRIMARY KEY IDENTITY, 
	[HMIId] int not null,
    [UserId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [ApproverId] INT NOT NULL, 
    [Notes] VARCHAR(200) NOT NULL
)
