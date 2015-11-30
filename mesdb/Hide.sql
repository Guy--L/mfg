CREATE TABLE [dbo].[Hide]
(
	[HideId] INT NOT NULL PRIMARY KEY, 
    [DateTime] DATETIME NOT NULL, 
    [HideType] INT NOT NULL, 
    [CoA] NCHAR(10) NOT NULL
)
