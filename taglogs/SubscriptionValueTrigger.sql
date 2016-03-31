CREATE TRIGGER [SubscriptionValueTrigger]
	ON [dbo].[Current]
	FOR UPDATE
	AS
	BEGIN
		update s
			set s.value = i.value
			, s.stamp = i.stamp
			from subscription s
			right join inserted i on i.tagid = s.tagid
			where s.SubscriptionId is not null

		SET NOCOUNT ON
	END
