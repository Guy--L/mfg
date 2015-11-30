CREATE TABLE [dbo].[BufferedHide]
(
	[BufferedHideId] INT NOT NULL PRIMARY KEY, 
    [WashTime] INT NOT NULL, 
    [WashTemp] INT NOT NULL, 
    [BufferTime] INT NOT NULL, 
    [WaterFlow] DECIMAL(18,4) NOT NULL, 
    [HideTypeId] INT NOT NULL, 
    [GrindBatchId] INT NOT NULL, 
    [HideId] INT NOT NULL, 
    CONSTRAINT [FK_BufferedHide_HideTypeId] FOREIGN KEY ([HideTypeId]) REFERENCES [HideType]([HideTypeId]), 
    CONSTRAINT [FK_BufferedHide_GrindBatch] FOREIGN KEY ([GrindBatchId]) REFERENCES [GrindBatch]([GrindBatchId]), 
    CONSTRAINT [FK_BufferedHide_Hide] FOREIGN KEY ([HideId]) REFERENCES [Hide]([HideId])
)
