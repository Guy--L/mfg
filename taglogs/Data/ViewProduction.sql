create view Production
as
select tagid, value, stamp from [past]
union all
select tagid, value, stamp from [all]
