SELECT DISTINCT g.TestGroupId, g.TimeStamp 
 FROM dbo.TestGroup g
 JOIN dbo.LineTest t ON t.TestGroupId = g.TestGroupId
 JOIN dbo.line n ON t.LineId = n.LineId
 WHERE n.UnitId = @0
 ORDER BY g.TimeStamp
 