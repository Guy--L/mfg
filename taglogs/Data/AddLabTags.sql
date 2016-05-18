
insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'csg_glyc_pct', 'x', 'Float', 0, 1 
from device d
where d.name = 'Lab'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'csg_moist_pct', 'x', 'Float', 0, 1 
from device d
where d.name = 'Lab'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'csg_oil_pct', 'x', 'Float', 0, 1 
from device d
where d.name = 'Lab'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'product_code', 'x', 'String', 0, 1 
from device d
where d.name = 'HMI'

insert into tag (deviceid, name, [address], datatype, islogged, isarchived)
select d.deviceid, 'line_status', 'x', 'String', 0, 1 
from device d
where d.name = 'HMI'

