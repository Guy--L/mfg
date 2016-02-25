merge lineoperation as target
using (
select		s.[INDAY]
           ,s.[INUNIT]
           ,s.[INLINE]
           ,s.[INSHFT]
           ,s.[STCODE]
           ,s.[INTIME]
           ,s.[RSCODE]
           ,s.[INPRD]
		,dbo.J2DateTime(s.[INDAY], s.[INTIME]) as stamp
           ,l.[LineId]
           ,0 as [ProductCodeId]
from devusa.devusa.ncifiles#.indtp700  s
join (select max(inday-1) recent from lineoperation) r on r.recent <= s.inday
join unit u on u.Unit = s.INUNIT
join line l on cast(s.[INLINE] as int) = l.LineNumber and u.UnitId = l.UnitId
) as source
on source.inday = target.inday
and source.inline = target.inline 
and source.inunit = target.inunit
and source.[INSHFT] = target.[INSHFT] 
and source.[STCODE] = target.[STCODE]
and source.[INTIME] = target.[INTIME] 
and source.[RSCODE] = target.[RSCODE]
and source.[INPRD] = target.[INPRD] 
when not matched by target then
INSERT     ([INDAY]
           ,[INUNIT]
           ,[INLINE]
           ,[INSHFT]
           ,[STCODE]
           ,[INTIME]
           ,[RSCODE]
           ,[INPRD]
           ,[stamp]
           ,[LineId]
           ,[ProductCodeId])
values (
		source.[INDAY]
       ,source.[INUNIT]
       ,source.[INLINE]
       ,source.[INSHFT]
       ,source.[STCODE]
       ,source.[INTIME]
       ,source.[RSCODE]
       ,source.[INPRD]
	   ,source.[stamp]
	   ,source.[LineId]
	   ,source.[ProductCodeId]);
