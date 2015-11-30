update [dbo].[TensileSample] t
	set t.[Completed] = @0
	where t.[TensileSampleId] = @1