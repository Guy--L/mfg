CREATE TABLE [dbo].[ReadingField]
(
	[ReadingFieldId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [FieldName] VARCHAR(20) NULL, 
    [Title] VARCHAR(40) NULL, 
    [LineColor] VARCHAR(20) NULL, 
    [Axis] INT NULL, 
    [Low] FLOAT NULL, 
    [High] FLOAT NULL
)
