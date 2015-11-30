CREATE TRIGGER dbo.HMIReset 
   ON  dbo.HMI
   AFTER UPDATE
AS 
BEGIN
	update i
	set i.requestcomplete = 0, i.requestpending = 1
	from HMI i
	join deleted d on d.HMIId = i.HMIId
	where (d.RequestComplete = 0 and i.RequestComplete = 1) or i.[Error] = 1
END