SELECT  t.[SolutionTestId]
      , t.[SolutionBatchId]
      , t.[DateTime]
      , t.[SolutionRecipeId]
      , t.[CMC]
      , t.[DensitySetPoint]
      , t.[ConsoleDensity]
      , t.[pHSetPoint]
      , t.[Viscoscity]
      , t.[Temperature]
      , t.[TitrationMLs]
      , t.[NaOCl Pump Set]
      , t.[NaOCl Flow]
      , row_number() over (partition by cast(t.[DateTime] as date) order by t.[DateTime]) ReadingNumber
      , t.[MeasuredDensity]
      , t.[ConsolepH]
      , t.[MeasuredpH]
	  , t.[Feed]
	  , t.[Steam]
      , t.[Conductivity]
      , t.[Acid Pump Output]
      , t.[Booster Pump Output]
      , t.[Glycerin]
	  , t.[Hypochlorite]
	  , t.[CasingGlycerin]
	  , b.[datetime] batchstamp
      , b.systemid
	  , s.[system]
      , r.solutiontype
    from solutiontest t
    inner join solutionbatch b on b.solutionbatchid = t.solutionbatchid
    inner join solutionrecipe r on b.solutionrecipeid = r.solutionrecipeid
    inner join [system] s on b.systemid = s.systemid
    WHERE b.solutionbatchid = @0
    order by b.systemid, b.datetime DESC
