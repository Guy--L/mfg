create view Production
as
SELECT        cast(pastid AS bigint) AS prdid, tagid, value, stamp
FROM            [past]
UNION ALL
SELECT        cast(allid AS bigint) + 2147483647 AS prdid, tagid, value, stamp
FROM            [all]