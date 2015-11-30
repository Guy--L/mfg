CREATE TABLE [dbo].[Reason]
(
	[Id] INT NOT NULL PRIMARY KEY identity, 
    [Code] CHAR(3) NOT NULL, 
    [Defect] VARCHAR(30) NOT NULL, 
    [Description] VARCHAR(50) NOT NULL, 
    [QualityEventId] INT NOT NULL, 
    CONSTRAINT [FK_Reason_QualityEvent] FOREIGN KEY ([QualityEventId]) REFERENCES [QualityEvent]([QualityEventId])

)
