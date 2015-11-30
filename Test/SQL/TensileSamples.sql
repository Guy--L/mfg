select  t.tensilesampleid
,t.stamp
,t.lineid
,n.unitid
,n.linenumber
,t.productcodeid
,t.note
,t.[Completed]
,t.tech
,p.productcode
,p.productspec
,p.productcode+' '+p.productspec as codespec
,p.wettensileminimum
,(select count(s.tensiletestid) from tensiletest s where t.tensilesampleid = s.TensileSampleId) as TestCount
from TensileSample t
join ProductCode p on t.ProductCodeId = p.ProductCodeId
join Line n on n.LineId = t.LineId