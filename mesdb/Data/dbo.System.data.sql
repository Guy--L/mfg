set identity_insert [System] on

MERGE INTO [System] AS Target 
USING (VALUES 
   (1, N'Good      ')
  ,(2, N'Good      ')
  ,(3, N'Dead      ')
  ,(4, N'Good      ')
  ,(5, N'Good      ')
  ,(6, N'Good      ')
  ,(7, N'Good      ')
  ,(8, N'Good      ')
  ,(9, N'Good      ')
) 
AS Source ([SystemId], [Status]) 
ON Target.[SystemId] = Source.[SystemId] 
-- update matched rows 
WHEN MATCHED THEN 
UPDATE SET [Status] = Source.[Status] 
-- insert new rows 
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([SystemId], [Status])
VALUES ([SystemId], [Status]) 
-- delete rows that are in the target but not the source 
WHEN NOT MATCHED BY SOURCE THEN 
DELETE;

set identity_insert [System] off
