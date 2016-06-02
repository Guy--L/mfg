INSERT INTO [dbo].[LineOperation]
           ([INDAY]
           ,[INUNIT]
           ,[INLINE]
           ,[INSHFT]
           ,[STCODE]
           ,[INTIME]
           ,[RSCODE]
           ,[INPRD]
           ,[stamp]
           ,[LineId]
)
select d.[INDAY]
       ,d.[INUNIT]
       ,d.[INLINE]
       ,d.[INSHFT]
       ,d.[STCODE]
       ,d.[INTIME]
       ,d.[RSCODE]
       ,d.[INPRD]
	   ,dbo.J2DateTime(d.[INDAY], d.[INTIME]) as stamp
	   ,l.LineId
from devusa.devusa.ncifiles#.indtp700 d
join unit u on u.Unit = d.INUNIT
join line l on cast(d.[INLINE] as int) = l.LineNumber and u.UnitId = l.UnitId
left join lineoperation n on n.lineid = l.LineId and n.inday = d.inday and n.rscode = d.rscode and n.inprd = d.inprd
where (d.inday*10000 + d.intime) > (select max((inday-1)*10000 + intime) from lineoperation) and n.lineid is null
		-- overlapped times to cover entries made later 
