CREATE TABLE [dbo].[Slide]
(
	[SlideId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DeckId] INT NOT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    [FileNameSuffix] VARCHAR(50) NOT NULL, 
    [FileFormat] VARCHAR(5) NULL
)
