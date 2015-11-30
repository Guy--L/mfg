CREATE trigger [dbo].[Unplugged]
on [dbo].[HMI]
for update
as begin
	update dbo.HMI set RequestPending = 1
	from inserted i
	join deleted d on i.HMIId = d.HMIId
	where i.HMIId = HMI.HMIId and i.Error = 1 and d.Error = 0
	
	update dbo.HMI set RequestPending = 2
	from inserted i
	join deleted d on i.HMIId = d.HMIId
	where i.HMIId = HMI.HMIId and i.Error = 0 and d.Error = 1

end