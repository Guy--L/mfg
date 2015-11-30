CREATE TABLE [dbo].[Product]
(
	[ProductId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Description] VARCHAR(50) NOT NULL, 
    [Diameter] INT NOT NULL, 
    [ColorId] INT NULL, 
    [PurposeId] INT NOT NULL, 
    [Strength] INT NOT NULL, 
    [Length] INT NOT NULL, 
    [SlugLength] INT NOT NULL, 
    [RigidityId] INT NOT NULL, 
    [UnshirrResponseId] INT NOT NULL, 
    [PCodeReference] VARCHAR(10) NULL, 
    CONSTRAINT [FK_Product_Color] FOREIGN KEY ([ColorId]) REFERENCES [Color]([ColorId]), 
    CONSTRAINT [FK_Product_Purpose] FOREIGN KEY ([PurposeId]) REFERENCES [Purpose]([PurposeId]), 
    CONSTRAINT [FK_Product_Rigidity] FOREIGN KEY ([RigidityId]) REFERENCES [Rigidity]([RigidityId]), 
    CONSTRAINT [FK_Product_UnshirrResponse] FOREIGN KEY ([UnshirrResponseId]) REFERENCES [UnshirrResponse]([UnshirrResponseId])
)
