CREATE TABLE [dbo].[Extruder]
(
	[ExtruderId] INT NOT NULL PRIMARY KEY, 
    [ExtruderType] INT NOT NULL, 
    [Nozzle] INT NOT NULL, 
    [Color] VARCHAR(20) NULL
)
