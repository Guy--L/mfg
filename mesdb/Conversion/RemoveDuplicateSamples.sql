delete s from sample s
join (
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
) t on t.sampleid = s.sampleid
where t.latest > 1

delete r 
from Reading r
left join [Sample] s on r.SampleId = s.SampleId
where s.SampleId is null
