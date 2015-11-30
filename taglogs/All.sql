CREATE TABLE [dbo].[All]
(
	[AllId] INT NOT NULL PRIMARY KEY IDENTITY,
	[TagId] [int] NOT NULL,
	[Value] [varchar](64) NOT NULL,
	[Stamp] [datetime] NOT NULL,
	[Quality] [int] NOT NULL
)
