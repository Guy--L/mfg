CREATE TABLE [dbo].[Line]
(
	[LineId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Status] CHAR NULL, 
    [LineTankId] INT NULL, 
    [UnitId] INT NOT NULL, 
    [LineNumber] INT NOT NULL, 
    CONSTRAINT [FK_Line_LineTank] FOREIGN KEY ([LineTankId]) REFERENCES [LineTank]([LineTankId]), 
    CONSTRAINT [FK_Line_Unit] FOREIGN KEY ([UnitId]) REFERENCES [Unit]([UnitId])
)
