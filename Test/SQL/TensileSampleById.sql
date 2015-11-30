            SELECT b.[TensileSampleId]
                  ,b.[Stamp]
                  ,b.[ProductCodeId]
                  ,b.[LineId]
				  ,s.[LineNumber]
                  ,b.[Tech]
                  ,b.[Note]
				  ,b.[Completed]
                  ,r.[ProductCode]
				  ,r.[ProductSpec]
                  ,r.[ProductCode]+r.[ProductSpec] as CodeSpec
				  ,r.[WetTensileMinimum]
				  ,s.[UnitId]
              FROM [dbo].[TensileSample] b
              join [dbo].[Line] s on b.[LineId] = s.[LineId]
              join [dbo].[ProductCode] r on b.[ProductCodeId] = r.[ProductCodeId]
			  where b.[TensileSampleId] = {0}
