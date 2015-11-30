set identity_insert Unit on

MERGE INTO Unit AS Target 
USING (VALUES 
  (1, N'A'),
  (2, N'B'),
  (3, N'C'),
  (4, N'D') 
) 
AS Source ([UnitId], [Unit]) 
ON Target.[UnitId] = Source.[UnitId] 
-- update matched rows 
WHEN MATCHED THEN 
UPDATE SET [Unit] = Source.[Unit] 
-- insert new rows 
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([UnitId], [Unit])
VALUES ([UnitId], [Unit]) 
-- delete rows that are in the target but not the source 
WHEN NOT MATCHED BY SOURCE THEN 
DELETE;

set identity_insert Unit off
