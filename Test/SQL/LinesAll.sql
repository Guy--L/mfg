SELECT LineId ,
       [Status] ,
       LineTankId ,
       UnitId ,
       LineNumber
 FROM dbo.line
 ORDER by unitid, lineid