delete from linetx
dbcc checkident(linetx, reset, 0)

declare @stamp datetime
declare @unitid int
declare @systemid int
declare @statusid int
declare @productcodeid int
declare @lineid int

-- use a cursor here because we want trigger to insert the product codes, line status changes and spec information into the tag database.
-- also linetx will be added by the trigger.  A normal update query does not step through all the transactions but optimizes to the last 
-- effective update to the line table.

declare tx cursor for
select stamp, unitid, systemid, statusid, s.productcodeid, lineid
from lineoperation p
join productcode s on p.productcodeid = s.ProductCodeId
join SolutionRecipe r on r.SolutionType = s.PlastSpec
join SolutionBatch b on b.SolutionRecipeId = r.SolutionRecipeId 
	and coalesce(b.[DateTime], dateadd(year, -200, getdate())) <= p.stamp 
	and p.stamp <= coalesce(b.Completed, dateadd(year, 200, getdate()))
join [Status] q on q.Code = p.RSCODE
join unit u on u.Unit = p.inunit
where year(p.stamp) = 2016
order by p.stamp asc

open tx
fetch next from tx into @stamp, @unitid, @systemid, @statusid, @productcodeid, @lineid

while @@fetch_status = 0
begin
	update n set 
	 stamp = @stamp
	,unitid = @unitid
	,systemid = @systemid
	,statusid = @statusid
	,productcodeid = @productcodeid
	from line n 
	where n.lineid = @lineid
	fetch next from tx into @stamp, @unitid, @systemid, @statusid, @productcodeid, @lineid
end

close tx
deallocate tx

SELECT t1.*
FROM lineoperation t1
LEFT OUTER JOIN lineoperation t2
  ON (t1.lineid = t2.lineid AND t1.stamp < t2.stamp)
WHERE t2.lineid IS NULL;

select * from linetx order by stamp desc