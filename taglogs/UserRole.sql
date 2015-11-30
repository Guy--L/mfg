CREATE TABLE [dbo].[UserRole]
(
	[UserRoleId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL, 
    [RoleId] INT NOT NULL, 
    CONSTRAINT [FK_UserRole_ToUser] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [FK_UserRole_ToRole] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId])
)
