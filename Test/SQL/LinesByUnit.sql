SELECT LineId ,
       [Status] ,
       LineTankId ,
       UnitId ,
       LineNumber
 FROM dbo.line
 WHERE unitid = @0
 ORDER by lineid