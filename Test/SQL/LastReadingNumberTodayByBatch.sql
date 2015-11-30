SELECT top 1 row_number() over (partition by cast(t.[DateTime] as date) order by t.[DateTime]) ReadingNumber
    from solutiontest t
    inner join solutionbatch b on b.solutionbatchid = t.solutionbatchid
    WHERE b.solutionbatchid = @0 and convert(date, getdate()) = cast(t.[DateTime] as date)
    order by t.datetime desc
