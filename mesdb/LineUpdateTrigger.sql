-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
alter TRIGGER [dbo].[RecordLineUpdate]
   ON  [dbo].[Line] 
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if update(productcodeid) 
	begin
        declare @insertedtags table
        (
	        tagid int, 
			lineid int,
	        productcodeid int
        )
		
		merge taglogs.dbo.[all] as target
		using (
			select t.tagid, p.productcode+' '+p.productspec as product, getdate() as stamp, 192 as quality, p.productcodeid, i.lineid
			from inserted i 
			join [ProductCode] p on p.ProductCodeId = i.ProductCodeId 
			join [Unit] u on u.unitid = i.UnitId
			join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
			join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
			join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
			where t.name = 'product_code' and d.Name = 'HMI'
	    ) as source
		on 1 = 0
		when not matched then
			insert (tagid, value, stamp, quality)
			values (tagid, product, stamp, quality)
	    output inserted.tagid, source.lineid, source.productcodeid into @insertedtags;

		insert into taglogs.dbo.[Limit]
		select r.tagid, getdate(), p.reelmoist_min, p.reelmoist_min + 0.5, p.reelmoist_aim, p.reelmoist_max - 0.5, p.reelmoist_max
		from [ReadingTag] r
		join [ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
		join @insertedtags i on i.lineid = r.LineId
		join [ProductCode] p on p.ProductCodeId = i.productcodeid
		where f.FieldName = 'csg_moist_pct'

		insert into taglogs.dbo.[Limit]
		select r.tagid, getdate(), p.gly_min, p.gly_aim - 1.0, p.gly_aim, p.gly_aim + 1.0, p.gly_max
		from [ReadingTag] r
		join [ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
		join @insertedtags i on i.lineid = r.LineId
		join [ProductCode] p on p.ProductCodeId = i.productcodeid
		where f.FieldName = 'csg_glyc_pct'
	end

	if update(statusid)
	begin
		insert into taglogs.dbo.[all]
		select t.tagid, s.code, getdate(), 192
		from inserted i 
		join [Status] s on s.StatusId = i.StatusId 
		join [Unit] u on u.unitid = i.UnitId
		join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
		join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
		join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
		where t.name = 'line_status'
	end

	--insert into linetx 
	--select @updated.lineid, 'trigger', 
END
