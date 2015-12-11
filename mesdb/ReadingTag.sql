CREATE TABLE [dbo].[ReadingTag]
(
	[ReadingTagId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ReadingFieldId] INT NOT NULL, 
    [LineId] INT NOT NULL, 
    [TagId] INT NOT NULL
)
