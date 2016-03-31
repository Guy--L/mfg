-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER dbo.NameNewCurrent
   ON  dbo.[Current] 
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	update n
		set n.name = c.name + '.' + d.name + '.' + t.name
		from [current] n
		join inserted i on n.tagid = i.tagid
		join tag t on n.tagid = t.tagid
		join device d on d.deviceid = t.deviceid
		join channel c on c.ChannelId = d.ChannelId
END
GO
