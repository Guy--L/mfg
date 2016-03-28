use mesdb
go

update x set 
 personid = 0
,stamp = p.stamp
,unitid = u.unitid
,systemid = b.systemid
,statusid = x.statusid
,productcodeid = p.productcodeid
,conversionid = 0
from line x
join lineoperation p on p.lineid = x.lineid
join productcode s on p.productcodeid = s.ProductCodeId
join SolutionRecipe r on r.SolutionType = s.PlastSpec
join SolutionBatch b on b.SolutionRecipeId = r.SolutionRecipeId 
	and coalesce(b.[DateTime], dateadd(year, -200, getdate())) <= p.stamp 
	and p.stamp <= coalesce(b.Completed, dateadd(year, 200, getdate()))
join [Status] t on t.Code = p.RSCODE
join unit u on u.Unit = p.inunit
where year(p.stamp) = 2016

