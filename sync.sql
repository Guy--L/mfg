update n 
set n.productcodeid = coalesce(
	(select top 1 productcodeid
	from [plan]
	where stamp <= n.stamp and Code = n.inprd 
	order by stamp desc),0
)
from lineoperation n
where n.INDAY > 23000 and n.productcodeid = 0

select * from [plan] where year(stamp) = 2016 and month(stamp) = 4

delete from [plan]
DBCC CHECKIDENT ('[plan]', RESEED, 0)

select min(stamp) from [plan]

select count(*) from lineoperation where inday > 23000 and productcodeid = 0 and stcode = 'U'

select * from lineoperation where  inday > 23000 and productcodeid = 0 order by stamp desc
select * from productcode where productcodeid = 898
select * from productcode where productcode = '30D01R'
select * from [lineoperation] where recordid = 36230

delete from [plan]

update p
set p.productcodeid = x.productcodeid 
from [plan] p
join productcode x on x.ProductCode = p.Code and x.ProductSpec = p.Spec

select count(*) from [plan] where productcodeid != 0 and code like '%X%'
select count(*) from [plan] where code like '%X%'

set identity_insert ProductCode Off
insert into ProductCode            (
			[ProductCodeId]
		   ,[ProductCode]
           ,[ProductSpec]
           ,[IsActive]
           ,[PlastSpec]
           ,[WetLayflat_Aim]
           ,[WetLayflat_Min]
           ,[WetLayflat_Max]
           ,[Glut_Aim]
           ,[Glut_Max]
           ,[Glut_Min]
           ,[FFW_Aim]
           ,[FFW_Min]
           ,[FFW_Max]
           ,[CasingWt_Aim]
           ,[CasingWt_Min]
           ,[CasingWt_Max]
           ,[ShirrMoist_Aim]
           ,[ShirrMoist_Min]
           ,[ShirrMoist_Max]
           ,[ReelMoist_Aim]
           ,[ReelMoist_Min]
           ,[ReelMoist_Max]
           ,[LFShirr_Aim]
           ,[LFShirr_Min]
           ,[LFShirr_Max]
           ,[LFShirr_LCL]
           ,[LFShirr_UCL]
           ,[LF_Aim]
           ,[LF_Min]
           ,[LF_Max]
           ,[LF_LCL]
           ,[LF_UCL]
           ,[OilType]
           ,[Oil_Aim]
           ,[Oil_Min]
           ,[Oil_Max]
           ,[Gly_Aim]
           ,[Gly_Min]
           ,[Gly_Max]
           ,[DryTensShirr_Min]
           ,[Unshirr_Max]
           ,[Unshirr_Avg]
           ,[WetTens_Min]
           ,[BlowShirr_Aim]
           ,[BlowShirr_Min]
           ,[BlowShirr_Max]
           ,[DT_LCL])
select
			[ProductCodeId]
		   ,[ProductCode]
           ,[ProductSpec]
           ,[IsActive]
           ,[PlastSpec]
           ,[WetLayflat_Aim]
           ,[WetLayflat_Min]
           ,[WetLayflat_Max]
           ,[Glut_Aim]
           ,[Glut_Max]
           ,[Glut_Min]
           ,[FFW_Aim]
           ,[FFW_Min]
           ,[FFW_Max]
           ,[CasingWt_Aim]
           ,[CasingWt_Min]
           ,[CasingWt_Max]
           ,[ShirrMoist_Aim]
           ,[ShirrMoist_Min]
           ,[ShirrMoist_Max]
           ,[ReelMoist_Aim]
           ,[ReelMoist_Min]
           ,[ReelMoist_Max]
           ,[LFShirr_Aim]
           ,[LFShirr_Min]
           ,[LFShirr_Max]
           ,[LFShirr_LCL]
           ,[LFShirr_UCL]
           ,[LF_Aim]
           ,[LF_Min]
           ,[LF_Max]
           ,[LF_LCL]
           ,[LF_UCL]
           ,[OilType]
           ,[Oil_Aim]
           ,[Oil_Min]
           ,[Oil_Max]
           ,[Gly_Aim]
           ,[Gly_Min]
           ,[Gly_Max]
           ,[DryTensShirr_Min]
           ,[Unshirr_Max]
           ,[Unshirr_Avg]
           ,[WetTens_Min]
           ,[BlowShirr_Aim]
           ,[BlowShirr_Min]
           ,[BlowShirr_Max]
           ,[DT_LCL] from [nitta-mfg01\ncimes].mesdb.dbo.ProductCode

select max(productcodeid) from ProductCode
