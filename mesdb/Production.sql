CREATE TABLE [dbo].[Production]
(
	[ProductionId] INT NOT NULL PRIMARY KEY, 
    [LineId] INT NOT NULL, 
    [ProductBatchId] INT NOT NULL, 
    CONSTRAINT [FK_Production_Line] FOREIGN KEY ([LineId]) REFERENCES [Line]([LineId]), 
    CONSTRAINT [FK_Production_ProductBatch] FOREIGN KEY ([ProductBatchId]) REFERENCES [ProductBatch]([ProductBatchId])
)
