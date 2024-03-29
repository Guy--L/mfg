USE [Order]
GO
SET IDENTITY_INSERT [dbo].[Product] ON 

INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (1, N'Test Product', 19, NULL, 1, 45, 60, 12, 1, 1, N'23D08R')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (2, N'Breakfast Casing', 19, NULL, 1, 70, 54, 12, 2, 2, N'23J18')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (3, N'Snack 102', 16, 4, 4, 65, 60, 12, 1, 1, N'21D19')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (6, N'Roll 15', 19, NULL, 11, 66, 60, 12, 1, 1, N'28D22')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (7, N'Kosher 200', 19, NULL, 3, 50, 60, 12, 1, 1, N'30J06')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (8, N'Kosher 300', 19, NULL, 3, 60, 60, 16, 1, 2, N'19D32')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (9, N'Snack 103', 19, NULL, 4, 45, 60, 16, 1, 1, N'19B24')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (10, N'Snack 104', 19, NULL, 4, 77, 45, 12, 2, 2, N'19G13')
INSERT [dbo].[Product] ([ProductId], [Description], [Diameter], [ColorId], [PurposeId], [Strength], [Length], [SlugLength], [RigidityId], [UnshirrResponseId], [PCodeReference]) VALUES (11, N'Roll 16', 12, NULL, 11, 66, 60, 12, 1, 1, NULL)
SET IDENTITY_INSERT [dbo].[Product] OFF
