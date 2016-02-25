delete from [All] where allid in (
select * from [All] a
join Tag t on t.Tagid = a.Tagid
where t.Name in ('product_code', 'csg_moist_pct', 'csg_glyc_pct', 'line_status')
)

select * from Limit

dbcc checkident(limit, reseed, 0)