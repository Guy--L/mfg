CREATE TABLE [dbo].[User]
(
	[UserId] INT NOT NULL PRIMARY KEY identity,
	[EmployeeNumber] NVARCHAR (10) NULL,
    [FirstName]      NVARCHAR (30) NOT NULL,
    [LastName]       NVARCHAR (30) NOT NULL,
    [IsManager]      BIT           NOT NULL,
    [IsActive]       BIT           NOT NULL,
    [IsPartTime]     BIT           NOT NULL,
	[Email]			 NVARCHAR (60) NULL,
    [IsAdmin]        BIT           NOT NULL,
    [ManagerId]      INT           NULL
)
