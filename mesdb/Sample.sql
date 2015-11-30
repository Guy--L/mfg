CREATE TABLE [dbo].[Sample]
(
	[SampleId] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Scheduled] datetime not null,
    [Stamp] DATETIME NOT NULL, 
    [LineId] INT NOT NULL, 
    [ProductCodeId] INT NOT NULL, 
    [Note] VARCHAR(64) NOT NULL, 
    [Tech] CHAR(2) NOT NULL, 
    [Completed] DATETIME NULL, 
    [ReelNumber] INT NOT NULL, 
    [Footage] INT NULL, 
    [BarCode] INT NULL, 
    [ParameterId] INT NULL, 
    [Reading1] INT NULL, 
    [Reading2] INT NULL, 
    [ProcessId] INT NULL, 
    [SystemId] INT NULL, 
    [NextScheduled] DATETIME NULL, 
    [Reading3] INT NULL, 
    CONSTRAINT [FK_Sample_ToLine] FOREIGN KEY ([LineId]) REFERENCES [Line]([LineId]), 
    CONSTRAINT [FK_Sample_ToProductCode] FOREIGN KEY ([ProductCodeId]) REFERENCES [ProductCode]([ProductCodeId])
)
