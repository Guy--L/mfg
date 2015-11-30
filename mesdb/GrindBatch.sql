CREATE TABLE [dbo].[GrindBatch]
(
	[GrindBatchId] INT NOT NULL PRIMARY KEY, 
    [DateTime] DATETIME NOT NULL, 
    [HideId] INT NOT NULL, 
    [CoA] NCHAR(10) NOT NULL, 
    [BufferedHideId] INT NOT NULL, 
    CONSTRAINT [FK_GrindBatch_Hide] FOREIGN KEY ([HideId]) REFERENCES [Hide]([HideId]), 
    CONSTRAINT [FK_GrindBatch_BufferedHide] FOREIGN KEY ([BufferedHideId]) REFERENCES [BufferedHide]([BufferedHideId])
)
