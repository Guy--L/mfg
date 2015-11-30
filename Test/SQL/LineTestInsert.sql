INSERT INTO [dbo].[LineTest]
            ([TimeStamp]
            ,[TestGroupId]
            ,[LineId]
            ,[SystemId]
            ,[StdSpeed]
            ,[Computer]
            ,[BeltSpeed]
            ,[DryEndSpeed]
            ,[ZoneA]
            ,[ZoneB]
            ,[ZoneC]
            ,[WetDoorNumber]
            ,[CasingTemp]
            ,[Delm]
            ,[CasingMoist]
            ,[CasingGlyc]
            ,[Gly]
            ,[CMC]
            ,[Cond]
            ,[pH]
            ,[HypoLevel]
            ,[Timing]
			,[OilSetpoint]
			,[OilActual]
            ,[Product]
            ,[Notes])
        VALUES
            (    '{0}' -- <TimeStamp, datetime,>
            ,    {1} -- <TestGroupId, int,>
            ,    {2} -- <LineId, int,>
            ,    {3} -- <SystemId, int,>
            ,    {4} -- <StdSpeed, int,>
            ,    {5} -- <Computer, int,>
            ,    {6} -- <BeltSpeed, int,>
            ,    {7} -- <DryEndSpeed, int,>
            ,    {8} -- <ZoneA, int,>
            ,    {9} -- <ZoneB, int,>
            ,   {10} -- <ZoneC, int,>
            ,   {11} -- <WetDoorNumber, int,>
            ,   {12} -- <CasingTemp, int,>
            ,   {13} -- <Delm, int,>
            ,   {14} -- <CasingMoist, int,>
            ,   {15} -- <CasingGlyc, int,>
            ,   {16} -- <Gly, int,>
            ,   {17} -- <CMC, int,>
            ,   {18} -- <Cond, int,>
            ,   {19} -- <pH, int,>
            ,   {20} -- <HypoLevel, int,>
            ,   {21} -- <Timing, int,>
			,	{22} -- <OilSetpoint, int,>
			,	{23} -- <OilActual, int,>
            , '{24}' -- <Product, varchar(6),>
            , '{25}') -- <Notes, varchar(50),>
