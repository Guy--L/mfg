;with t as (
select x.solutionbatchid, x.[datetime] from solutiontest x where x.[datetime] > dateadd(day, -30, getdate())
)
select t.solutionbatchid batchid, r.SolutionType recipe, min(t.[datetime]) [begin], max(t.[datetime]) [end]
from t
left join solutionbatch b on b.solutionbatchid = t.solutionbatchid
left join solutionrecipe r on r.SolutionRecipeId = b.SolutionRecipeId
group by t.solutionbatchid, b.systemid, r.SolutionType
having b.systemid = 1 
