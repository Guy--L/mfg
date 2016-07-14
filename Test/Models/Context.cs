using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class Context
    {
        private static string _code = @"
            select top 1 productcodeid 
            from productcode 
            where productcode = @0 and productspec not like 'I%' 
            order by productspec desc
        ";
        private static string _spec = @"
            select productcodeid 
            from productcode 
            where productcode = @0 and productspec = @1
        ";
        private static string _sample = @"
            select top 1 sampleid, p.productcodeid, productcode + ' ' + productspec as product, s.stamp as start, s.stamp as [end]
            from [sample] s
            join productcode p on p.productcodeid = s.productcodeid
            join line n on n.lineid = s.lineid
            join unit u on u.unitid = n.unitid
            where u.unit = @0
            and n.linenumber = @1
            and s.reelnumber = @2
            and s.stamp > dateadd(hour, -33, dateadd(day, @4, datetimefromparts(2010+@3, 1, 1, 1, 0, 0, 0)))
            and s.stamp < dateadd(hour, -9,  dateadd(day, @4, datetimefromparts(2010+@3, 1, 1, 1, 0, 0, 0)))
        ";
        private static string _statsByProduct = @"
            select coalesce(cnt, 0) c from (
                select 1 ord, count(*) cnt from [sample] where productcodeid = @0
                  union
                select 2, count(distinct s.SolutionTestId) cnt
                from [solutiontest] s
                join solutionbatch b on b.SolutionBatchId = s.SolutionBatchId
                join [system] n on n.SystemId = b.SystemId
                join LineTx x on x.SystemId = n.SystemId
                join [status] t on t.StatusId = x.StatusId
                where x.productcodeid = @0 and t.Code = 'RP'
                  union
                select 3, count(*) cnt from line where productcodeid = @0
                  union
                select 4, count(*) cnt from productcode where productcode = (select productcode from productcode where productcodeid = @0)
                  union
                select 5, count(*) cnt from [plan]
                where productcodeid = @0 
            ) t order by ord
        ";
        private static string _statsBySample = @"
            select coalesce(cnt, 0) c from (
                select 1 ord, count(*) cnt from [sample] s
                join [sample] r on s.reelnumber = r.reelnumber and s.lineid = r.lineid 
                and year(r.stamp) = year(s.stamp) and datepart(dayofyear, r.stamp) = datepart(dayofyear, s.stamp)
                where s.sampleid = @0
                  union
                select 2, count(distinct s.SolutionTestId) cnt
                from [solutiontest] s
                join solutionbatch b on b.SolutionBatchId = s.SolutionBatchId
                join [system] n on n.SystemId = b.SystemId
                join [sample] r on r.systemid = n.systemid
                where r.sampleid = @0
                  union
                select 3, 1 cnt
                  union
                select 4, count(*) cnt from productcode where productcode = (
                select productcode from productcode p
                join [sample] r on r.productcodeid = p.productcodeid    
                where r.sampleid = @0)
                  union
                select 5, 0 cnt
            ) m order by ord";

        public string ConnectionId { get; set; }
        public int ProductCodeId { get; set; }
        public int SampleId { get; set; }
        public int LineId { get; set; }
        public int SystemId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Product { get; set; }
        public string LotNum { get; set; }

        public List<int> Statistics { get; set; }

        public string SampleCnt { get { return Statistics?[0].ToString() + ' '; } }
        public string SolutionCnt { get { return Statistics?[1].ToString() + ' '; } }
        public string LineCnt { get { return Statistics?[2].ToString() + ' '; } }
        public string ProductCnt { get { return Statistics?[3].ToString() + ' '; } }
        public string ScheduleCnt { get { return Statistics?[4].ToString() + ' '; } }

        public Context()
        { }

        public Context(string code, string spec)
        {
            using (labDB d = new labDB())
            {
                ProductCodeId = spec == null ? d.ExecuteScalar<int>(_code, code) : d.ExecuteScalar<int>(_spec, code, spec);
            }
            Product = (code + " " + spec).Trim();
            Stats();
        }

        public Context(string lot)
        {
            LotNum = lot;
            if (lot.Length == 10)
                lot = lot.Substring(1);
            var year = lot[0] - '0';
            int julian = 0;
            if (year < 0 || year > 9 || !int.TryParse(lot.Substring(1, 3), out julian))
            {
                ProductCodeId = 0;
                SampleId = 0;
                return;
            }
            var unit = lot.Substring(4, 1);
            var line = lot.Substring(5, 1);
            var reel = lot.Substring(8, 1);
            using (labDB d = new labDB())
            {
                try
                {
                    d.SingleOrDefaultInto(this, _sample, unit, line, reel, year, julian);
                }
                catch (Exception e)
                {
                    var tst = e;
                }
            }
            Stats();
        }

        public List<int> Stats()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LotNum) && ProductCodeId > 0)
                    using (labDB d = new labDB())
                    {
                        Statistics = d.Fetch<int>(_statsByProduct, ProductCodeId);
                    }
                else if (SampleId > 0)
                    using (labDB d = new labDB())
                    {
                        Statistics = d.Fetch<int>(_statsBySample, SampleId);
                    }
            }
            catch (Exception e)
            {
                var tst = e;
            }
            return Statistics;
        }
    }
}