SELECT
 g.TestGroupId ,
 g.TimeStamp ,
 t.BeltSpeed ,
 t.CasingGlyc ,
 t.CasingMoist ,
 t.CasingTemp ,
 t.CMC ,
 t.Computer ,
 t.Cond ,
 t.Delm ,
 t.DryEndSpeed ,
 t.Gly ,
 t.HypoLevel ,
 t.LineId ,
 t.LineTestId ,
 t.Notes ,
 t.OilSetpoint ,
 t.OilActual ,
 t.pH ,
 t.Product ,
 t.StdSpeed ,
 t.SystemId ,
 t.TimeStamp ,
 t.Timing ,
 t.WetDoorNumber ,
 t.ZoneA ,
 t.ZoneB ,
 t.ZoneC ,
 n.LineNumber AS [LineNo]
 FROM dbo.TestGroup g
 JOIN dbo.LineTest t ON t.TestGroupId = g.TestGroupId
 JOIN dbo.line n ON t.LineId = n.LineId
 WHERE n.UnitId = @0 AND g.TestGroupId = @1
 ORDER BY n.LineNumber, t.TimeStamp