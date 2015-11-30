-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create TRIGGER [dbo].[UpdateLineStatus]
   ON  [dbo].[LineOperation] 
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

update d 
	set d.[INDAY] = s.[INDAY],
		d.[Stamp] = i.[stamp],
		d.[Reason] = i.[RSCODE],
		d.[Status] = i.[STCODE]
       ,d.[INPRD] = s.[INPRD]
       ,d.[CARTN] = s.[CARTN]
       ,d.[INSID] = s.[INSID]
       ,d.[INLSQ] = s.[INLSQ]
       ,d.[INLST] = s.[INLST]
       ,d.[INREL] = s.[INREL]
       ,d.[INBSP] = s.[INBSP]
       ,d.[INSAM] = s.[INSAM]
       ,d.[LineId] = l.[LineId]
	   ,d.[ProductCodeId] = i.ProductCodeId
from LineStatus d
join devusa.devusa.dcnitta.inltp856 s on s.[INLIN] = d.[INLIN] and s.[INUNT] = d.[INUNT]
join inserted i on i.[INLINE] = d.[INLIN] and i.[INUNIT] = d.[INUNT]
join Unit u on u.Unit = d.[INUNT]
join line l on u.UnitId = l.[UnitId] and cast(d.[INLIN] as int) = l.LineNumber

END