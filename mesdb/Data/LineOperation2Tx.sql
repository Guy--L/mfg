insert into linetx (lineid, personid, stamp, unitid, linenumber, systemid, statusid, productcodeid, conversionid)
select p.lineid, 0, p.stamp, u.unitid, p.inline, b.systemid, x.statusid, p.productcodeid, 0
from lineoperation p
join productcode s on p.productcodeid = s.ProductCodeId
join SolutionRecipe r on r.SolutionType = s.PlastSpec
join SolutionBatch b on b.SolutionRecipeId = r.SolutionRecipeId 
	and coalesce(b.[DateTime], dateadd(year, -200, getdate())) <= p.stamp 
	and p.stamp <= coalesce(b.Completed, dateadd(year, 200, getdate()))
join [Status] x on x.Code = p.RSCODE
join unit u on u.Unit = p.inunit
where year(p.stamp) = 2016
order by b.[DateTime] desc

