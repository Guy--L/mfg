select [SampleId]
      ,[Scheduled]
      ,[Stamp]
      ,[LineId]
      ,[ProductCodeId]
      ,[Note]
      ,[Tech]
      ,[Completed]
      ,[ReelNumber]
      ,[Footage]
      ,[BarCode]
      ,[ParameterId]
      ,[Reading1]
      ,[Reading2]
      ,[ProcessId]
      ,[SystemId]
      ,[NextScheduled]
      ,[Reading3]
into uniquesample
 from (
  select *, row_number() over (
	partition by 
       [Scheduled]
      ,[Stamp]
      ,[LineId]
      ,[ProductCodeId]
      ,[Note]
      ,[Tech]
      ,[ReelNumber]
      ,[Footage]
      ,[BarCode]
      ,[ParameterId]
      ,[Reading1]
      ,[Reading2]
      ,[ProcessId]
      ,[SystemId]
      ,[NextScheduled]
      ,[Reading3]
	order by [Completed] desc
	) as latest
	from [sample]
) t
where latest = 1

exec sp_rename 'sample', 'oldsample'
exec sp_rename 'uniquesample', 'sample'

select * into oldreading from Reading

delete r 
from Reading r
left join [Sample] s on r.SampleId = s.SampleId
where s.SampleId is null
