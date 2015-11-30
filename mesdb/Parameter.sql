CREATE TABLE [dbo].[Parameter]
(
	[ParameterId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(50) NOT NULL, 
    [Scale] VARCHAR(10) NOT NULL, 
    [Mask] VARCHAR(50) NOT NULL, 
    [Units] VARCHAR(10) NULL, 
    [Diary] VARCHAR(50) NOT NULL, 
    [Count] INT NOT NULL, 
    [Icon] VARCHAR(20) NULL, 
    [ReadNow] BIT NOT NULL, 
    [Cells] VARCHAR(50) NULL
)
