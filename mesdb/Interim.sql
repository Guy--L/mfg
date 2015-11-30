CREATE TABLE [dbo].[Interim]
(
	[InterimId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ProductSpecId] INT NOT NULL, 
    [DateTime] DATETIME NOT NULL, 
    [Description] VARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_Interim_ProductSpec] FOREIGN KEY ([ProductSpecId]) REFERENCES [ProductSpec]([ProductSpecId]) 
)
