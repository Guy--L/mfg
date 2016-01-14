CREATE TRIGGER [LineAssignmentTrigger]
	ON [dbo].[Line]
	after UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		if (update(productcodeid))
		begin
			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select reelmoist_min
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'csg_moist_low' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid

			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select reelmoist_max 
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'csg_moist_high' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid

			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select reelmoist_aim 
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'csg_moist_aim' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid

			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select reelmoist_min
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'csg_moist_low' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid

			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select reelmoist_max 
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'csg_moist_high' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid

			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select reelmoist_aim 
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'csg_moist_aim' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid
			
			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select productcode+' '+productspec 
				from productcode 
				where productcodeid = inserted.productcodeid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'product_code' and r.LineId = inserted.lineid
			and inserted.productcodeid != deleted.productcodeid
		end

		if (update(statusid))
		begin
			insert into taglogs.dbo.[All] 
			select r.tagid, (
				select code
				from [status] 
				where statusid = inserted.statusid),
			getdate(), 192
			from ReadingTag r
			join ReadingField f on r.ReadingFieldId = f.ReadingFieldId
			where f.FieldName = 'line_status' and r.LineId = inserted.lineid
		end
	END
