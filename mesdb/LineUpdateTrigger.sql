-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[RecordLineUpdate]
   ON  [dbo].[Line] 
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @archivecut datetime
	select @archivecut = min(stamp) from taglogs.dbo.[all]

	if update(productcodeid) 
	begin
        declare @insertedtags table
        (
	        tagid int, 
			lineid int,
	        productcodeid int,
			stamp datetime
        )

		merge taglogs.dbo.[all] as target
		using (
			select t.tagid, p.productcode+' '+p.productspec as product, i.stamp, 192 as quality, p.productcodeid, i.lineid
			from inserted i 
			join [ProductCode] p on p.ProductCodeId = i.ProductCodeId 
			join [Unit] u on u.unitid = i.UnitId
			join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
			join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
			join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
			where t.name = 'product_code' and d.Name = 'HMI'
	    ) as source
		on source.stamp < @archivecut
		when not matched then
			insert (tagid, value, stamp, quality)
			values (tagid, product, stamp, quality)
	    output inserted.tagid, source.lineid, source.productcodeid, inserted.stamp into @insertedtags;

		merge taglogs.dbo.[past] as target
		using (
			select t.tagid, p.productcode+' '+p.productspec as product, i.stamp, p.productcodeid, i.lineid
			from inserted i 
			join [ProductCode] p on p.ProductCodeId = i.ProductCodeId 
			join [Unit] u on u.unitid = i.UnitId
			join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
			join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
			join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
			where t.name = 'product_code' and d.Name = 'HMI'
	    ) as source
		on source.stamp >= @archivecut
		when not matched then
			insert (tagid, value, stamp)
			values (tagid, product, stamp)
	    output inserted.tagid, source.lineid, source.productcodeid, inserted.stamp into @insertedtags;

		insert into taglogs.dbo.[Limit]
		select r.tagid, i.stamp
			, coalesce(p.reelmoist_min,0)
			, coalesce(p.reelmoist_min + 0.5,0)
			, coalesce(p.reelmoist_aim,0)
			, coalesce(p.reelmoist_max - 0.5,0)
			, coalesce(p.reelmoist_max,0)
		from [ReadingTag] r
		join [ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
		join @insertedtags i on i.lineid = r.LineId
		join [ProductCode] p on p.ProductCodeId = i.productcodeid
		where f.FieldName = 'csg_moist_pct'

		insert into taglogs.dbo.[Limit]
		select r.tagid, i.stamp
			, coalesce(p.gly_min,0)
			, coalesce(p.gly_aim - 1.0,0)
			, coalesce(p.gly_aim,0)
			, coalesce(p.gly_aim + 1.0,0)
			, coalesce(p.gly_max,0)
		from [ReadingTag] r
		join [ReadingField] f on f.ReadingFieldId = r.ReadingFieldId
		join @insertedtags i on i.lineid = r.LineId
		join [ProductCode] p on p.ProductCodeId = i.productcodeid
		where f.FieldName = 'csg_glyc_pct'

		insert into taglogs.dbo.[Limit]
		select t.tagid, i.stamp
			, coalesce(p.LF_Min,0)
			, coalesce(p.LF_LCL,0)
			, coalesce(p.LF_Aim,0)
			, coalesce(p.LF_UCL,0)
			, coalesce(p.LF_Max,0)
		from [ProductCode] p 
		join @insertedtags i on p.ProductCodeId = i.productcodeid
		join [Unit] u on u.unitid = i.UnitId
		join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
		join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
		join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
		where t.name = 'layflat_mm_pv'
	end

	if update(statusid)
	begin
		insert into taglogs.dbo.[all]
		select t.tagid, s.code, i.stamp, 192
		from inserted i 
		join [Status] s on s.StatusId = i.StatusId 
		join [Unit] u on u.unitid = i.UnitId
		join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
		join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
		join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
		where t.name = 'line_status' and i.stamp >= @archivecut

		insert into taglogs.dbo.[past]
		select t.tagid, s.code, i.stamp
		from inserted i 
		join [Status] s on s.StatusId = i.StatusId 
		join [Unit] u on u.unitid = i.UnitId
		join taglogs.dbo.[Channel] c on (u.unit+cast(i.linenumber as char)) = c.name
		join taglogs.dbo.[Device] d on d.ChannelId = c.ChannelId
		join taglogs.dbo.[Tag] t on t.DeviceId = d.DeviceId
		where t.name = 'line_status' and i.stamp < @archivecut

	end

	insert into linetx ([LineId], [PersonId], [Stamp], [Comment], [LineTankId], [UnitId], [LineNumber], [SystemId], [StatusId], [ProductCodeId])
	select lineid, personid, stamp, '', linetankid, unitid, linenumber, systemid, statusid, productcodeid from inserted
END