SELECT t.[TensileTestId]
      ,t.[TensileSampleId]
      ,t.[Stamp]
      ,t.[Average]
      ,t.[Range]
      ,t.[N]
      ,t.[Tech]
	  ,case when t.[Average] >= p.WetTensileMinimum * 100 then 'true' else 'false' end as [Passed]
	  ,row_number() over(order by t.[Stamp]) as TestNumber
  FROM TensileTest t
  join TensileSample s on t.TensileSampleId = s.TensileSampleId
  join ProductCode p on s.ProductCodeId = p.ProductCodeId
  where t.tensilesampleid = {0}