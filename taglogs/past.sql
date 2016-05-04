CREATE TABLE [dbo].[Past]
(
	[PastId] INT NOT NULL PRIMARY KEY IDENTITY,
	[TagId] [int] NOT NULL,
	[Value] [varchar](64) NULL,
	[Stamp] [datetime] NOT NULL, 
    CONSTRAINT [FK_Past_ToTag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([TagId]),
)
