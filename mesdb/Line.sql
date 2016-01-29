CREATE TABLE [dbo].[Line]
(
	[LineId] int not null primary key identity,
    [LineTankId] INT NULL, 
    [UnitId] INT NOT NULL, 
    [LineNumber] INT NOT NULL, 
    [SystemId] INT NULL, 
    [StatusId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL, 
    [Stamp] DATETIME NOT NULL, 
    [PersonId] INT NOT NULL DEFAULT 0, 
    [ConversionId] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Line_Unit] FOREIGN KEY ([UnitId]) REFERENCES [Unit]([UnitId])
)
