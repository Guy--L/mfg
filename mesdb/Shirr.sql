CREATE TABLE [dbo].[Shirr]
(
	[ShirrId] INT NOT NULL PRIMARY KEY, 
    [PullRollId] INT NOT NULL, 
    [ShirrRollId] INT NOT NULL, 
    [CompressionRollId] INT NOT NULL, 
    [MandrelId] INT NOT NULL, 
    [WhisperAirPressure] INT NOT NULL, 
    [ProductLength] INT NOT NULL
)
