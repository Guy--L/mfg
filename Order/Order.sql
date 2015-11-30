CREATE TABLE [dbo].[Order]
(
	[OrderId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [OrderDate] DATETIME NOT NULL, 
    [ShipByDate] DATETIME NULL, 
    [ProductId] INT NOT NULL, 
    [SalesPersonId] INT NOT NULL, 
    [CustomerId] INT NOT NULL, 
    [Quantity] INT NOT NULL, 
    [Length] INT NOT NULL, 
    CONSTRAINT [FK_Order_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([ProductId])
)
