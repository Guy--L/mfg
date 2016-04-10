CREATE TABLE [dbo].[Plot]
(
	[PlotId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [GraphId] INT NOT NULL, 
    [TagId] INT NOT NULL, 
    [YAxis] BIT NOT NULL, 
    [Relabel] VARCHAR(50) NOT NULL, 
    [Scale] INT NOT NULL, 
    [MinY] INT NOT NULL, 
    [MaxY] INT NOT NULL, 
    CONSTRAINT [FK_Series_ToChart] FOREIGN KEY ([GraphId]) REFERENCES [Graph]([GraphId]), 
    CONSTRAINT [FK_Series_ToTag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([TagId])
)
