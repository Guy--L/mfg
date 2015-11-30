CREATE TABLE [dbo].[ProductGel]
(
	[ProductGelId] INT NOT NULL PRIMARY KEY, 
    [ProductBatchId] INT NOT NULL, 
    [GelBatchId] INT NOT NULL, 
    CONSTRAINT [FK_ProductGel_ProductBatch] FOREIGN KEY ([ProductBatchId]) REFERENCES [ProductBatch]([ProductBatchId]), 
    CONSTRAINT [FK_ProductGel_GelBatch] FOREIGN KEY ([GelBatchId]) REFERENCES [GelBatch]([GelBatchId])
)
