CREATE TABLE [dbo].[Series]
(
	[SeriesId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Title] VARCHAR(50) NULL, 
    [Field] VARCHAR(50) NOT NULL, 
    [YLabel] VARCHAR(50) NOT NULL, 
    [Legend] VARCHAR(50) NOT NULL, 
    [Height] INT NULL, 
    [ForeGround] VARCHAR(50) NULL 
)
