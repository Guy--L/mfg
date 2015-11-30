CREATE TABLE [dbo].[TagHistory]
(
	[TagHistoryId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TagId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [Started] BIT NOT NULL, 
    [Seconds] INT NOT NULL, 
    [Type] INT NOT NULL, 
    CONSTRAINT [FK_TagHistory_ToTag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([TagId])
)
