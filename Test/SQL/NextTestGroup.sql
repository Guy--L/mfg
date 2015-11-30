SELECT TOP 1 g.TestGroupId, g.TimeStamp 
 FROM dbo.TestGroup g
 JOIN dbo.LineTest t ON t.TestGroupId = g.TestGroupId
 JOIN dbo.line n ON t.LineId = n.LineId
 WHERE n.UnitId = @0 AND g.TimeStamp > (SELECT timestamp FROM dbo.TestGroup WHERE testgroupid = @1)
 ORDER BY g.TimeStamp DESC
 