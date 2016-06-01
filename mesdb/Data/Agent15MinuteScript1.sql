/* 1. copy ActiveNew database into wwwroot\app_data
** 2. select Lineload Query from ActiveNew into LLQ
** 3. insert new specs into ProductCode
** 4. take last 20 lineoperations, filter to null productcode
** 5. update said null using LLQ and productcode table
** 6. update linestatus using LLQ  
** 7. update productcodes in lineoperation where RP follows CV
** 8. update specs in productcode from LLQ
*/

exec master..xp_cmdshell 'copy \\nitta-fs01\root\QDLS\specs\activnew.mdb c:\inetpub\wwwroot\app_data\activnew.mdb'
drop table [LineLoad Query]
select * into [LineLoad Query] 
from openquery(product, 'select * from [LineLoad Query]')

insert into [ProductCode] (
	 ProductCode,
	 ProductSpec,
	 PlastSpec,
	 WetLayflat_Aim,
	 WetLayflat_Min,
	 WetLayflat_Max,
	 [Glut_Aim],
	 [Glut_Max],
	 [Glut_Min],
	 [FFW_Aim],
	 [FFW_Min],
	 [FFW_Max],
	 [CasingWt_Aim],
	 [CasingWt_Min],
	 [CasingWt_Max],
	 [ShirrMoist_Aim],
	 [ShirrMoist_Min],
	 [ShirrMoist_Max],
	 [ReelMoist_Aim],
	 [ReelMoist_Min],
	 [ReelMoist_Max],
	 [LFShirr_Aim],
	 [LFShirr_Min],
	 [LFShirr_Max],
	 [LFShirr_LCL],
	 [LFShirr_UCL],
	 [LF_Aim],
	 [LF_Min],
	 [LF_Max],
	 [LF_LCL],
	 [LF_UCL],
	 [OilType],
	 [Oil_Aim],
	 [Oil_Min],
	 [Oil_Max],
	 [Gly_Aim],
	 [Gly_Min],
	 [Gly_Max],
	 [DryTensShirr_Min],
	 [Unshirr_Max],
	 [Unshirr_Avg],
	 [WetTens_Min],
	 [BlowShirr_Aim],
	 [BlowShirr_Min],
	 [BlowShirr_Max],
	 [DT_LCL]
	 )
	 select 
	 p.[PCode] ,
	 p.[Spec],
	 p.[Plast Spec],
	 p.[Wet L/F (Aim)],
	 p.[Wet L/F (Min)],
	 p.[Wet L/F (Max)],
	 p.[Glut_Aim],
	 p.[Glut_USL],
	 p.[Glut_LSL],
	 p.[FFW_Aim],
	 p.[FFW_Min],
	 p.[FFW_Max],
	 p.[CasingWt_Aim],
	 p.[CasingWt_Min],
	 p.[CasingWt_Max],
	 p.[ShirrMoist_Aim],
	 p.[ShirrMoist_Min],
	 p.[ShirrMoist_Max],
	 p.[ReelMoist_Aim],
	 p.[ReelMoist_Min],
	 p.[ReelMoist_Max],
	 p.[LF at shirr-Aim],
	 p.[LF at shirr-Min],
	 p.[LF at shirr-Max],
	 p.[LF at shirr-LCL],
	 p.[LF at shirr-UCL],
	 p.[LF_Aim],
	 p.[LF_Min],
	 p.[LF_Max],
	 p.[LF_LCL],
	 p.[LF_UCL],
	 p.[Oil Type],
	 p.[Oil_Aim],
	 p.[Oil_Min],
	 p.[Oil_Max],
	 p.[Gly_Aim],
	 p.[Gly_Min],
	 p.[Gly_Max],
	 p.[DryTens_shirr_Min],
	 p.[At shirr unshirr force (max)],
	 p.[At shirr unshirr force (avg)],
	 p.[Wet Tens (Min)],
	 p.[Blow @shirr (Aim)],
	 p.[Blow @shirr (Min)],
	 p.[Blow @shirr (Max)],
	 p.[DT LCL]
	 from openquery(product, 'select * from [Active Specifications]') p
	 left join productcode c on c.ProductCode = p.PCode and c.ProductSpec = p.Spec
	 where c.ProductCodeId is null

;with cte as (
select top(20) inday, inline, inunit, productcodeid from lineoperation order by stamp desc
) select * into #nojoin from cte where productcodeid is null

update d 
	set d.ProductCodeId = r.ProductCodeId
from LineOperation d
join #nojoin n on n.inday = d.inday and n.inline = d.inline and n.inunit = d.inunit
left join [LineLoad Query] q on q.Line = d.INLINE and q.Unit = d.INUNIT and q.pcode = d.inprd
left join productcode r on q.PCode = r.ProductCode and q.Spec = r.ProductSpec
where d.ProductCodeId is null and r.ProductCodeId is not null

update l
set l.insid = cast(replace(q.spec, ' ', '') as varchar(4)), l.productcodeid = p.productcodeid
from linestatus l
join #nojoin n on right(n.inday, 4) = l.inday and n.INLINE = l.inlin and n.inunit = l.inunt
join [LineLoad Query] q on q.pcode = l.inprd
join productcode p on p.ProductCode = l.inprd and p.ProductSpec = q.Spec

;with q as (
select lineid, inprd, stamp, productcodeid, rscode,
rn = row_number() over (partition by lineid order by stamp desc) 
from lineoperation 
where year(stamp) >= 2016
)
update b set b.ProductCodeId = a.ProductCodeId
from q a
join q b on a.inprd = b.inprd and a.rn = b.rn+1 and a.lineid = b.lineid
where b.productcodeid is null and b.rscode = 'CV' and a.rscode = 'RP'

drop table #nojoin

update p
set p.[ReelMoist_Aim] = q.[ReelMoist_Aim],
	p.[ReelMoist_Min] = q.[ReelMoist_Min],
	p.[ReelMoist_Max] = q.[ReelMoist_Max],
	p.[Oil_Min] = q.[Oil_Min],
	p.[Oil_Max] = q.[Oil_Max],
	p.[Oil_Aim] = q.[Oil_Aim],
	p.[Gly_Aim] = q.[Gly_Aim],
	p.[Gly_Min] = q.[Gly_Min],
	p.[Gly_Max] = q.[Gly_Max]
from ProductCode p 
join [LineLoad Query] q on p.ProductCode = q.pcode and p.ProductSpec = q.spec
