CREATE TABLE [dbo].[Exempt]
(
	[ExemptId] INT NOT NULL PRIMARY KEY identity, 
    [ExemptCode] VARCHAR(10) NOT NULL, 
    [Diameter] INT NOT NULL
)
