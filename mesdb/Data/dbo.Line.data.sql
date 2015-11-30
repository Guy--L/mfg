set identity_insert [Line] on

MERGE INTO [Line] AS Target 
USING (VALUES 
   (1, NULL, NULL, 1, 1)
  ,(2, NULL, NULL, 1, 2)
  ,(3, NULL, NULL, 1, 3)
  ,(4, NULL, NULL, 1, 4)
  ,(5, NULL, NULL, 1, 5)
  ,(6, NULL, NULL, 1, 6)
  ,(7, NULL, NULL, 1, 7)
  ,(8, NULL, NULL, 1, 8)
  ,(9, NULL, NULL, 2, 1)
  ,(10, NULL, NULL, 2, 2)
  ,(11, NULL, NULL, 2, 3)
  ,(12, NULL, NULL, 2, 4)
  ,(13, NULL, NULL, 2, 5)
  ,(14, NULL, NULL, 2, 6)
  ,(15, NULL, NULL, 2, 7)
  ,(16, NULL, NULL, 2, 8)
  ,(17, NULL, NULL, 3, 1)
  ,(18, NULL, NULL, 3, 2)
  ,(19, NULL, NULL, 3, 3)
  ,(20, NULL, NULL, 3, 4)
  ,(21, NULL, NULL, 3, 5)
  ,(22, NULL, NULL, 3, 6)
  ,(23, NULL, NULL, 3, 7)
  ,(24, NULL, NULL, 3, 8)
  ,(25, NULL, NULL, 4, 1)
  ,(26, NULL, NULL, 4, 2)
  ,(27, NULL, NULL, 4, 3)
  ,(28, NULL, NULL, 4, 4)
  ,(29, NULL, NULL, 4, 5)
  ,(30, NULL, NULL, 4, 6)
  ,(31, NULL, NULL, 4, 7)
  ,(32, NULL, NULL, 4, 8)
) 
AS Source ([LineId], [Status], [LineTankId], [UnitId], [LineNumber]) 
ON Target.[LineId] = Source.[LineId] 
-- update matched rows 
WHEN MATCHED THEN 
UPDATE SET	[Status] = Source.[Status],
	[LineTankId] = Source.[LineTankId],
	[UnitId] = Source.[UnitId],
	[LineNumber] = Source.[LineNumber]
-- insert new rows 
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([LineId], [Status], [LineTankId], [UnitId], [LineNumber])
VALUES ([LineId], [Status], [LineTankId], [UnitId], [LineNumber]) 
-- delete rows that are in the target but not the source 
WHEN NOT MATCHED BY SOURCE THEN 
DELETE;

set identity_insert [Line] off