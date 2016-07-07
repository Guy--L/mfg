using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class Context
    {
        private string _code = @"
            select top 1 @@pid = productcodeid 
            from productcode
            where productcode = @0 
        ";
        private string _context = @"
            declare @@pid int

            select @@pid = productcodeid from productcode 
            where productcode = @0 and productspec = @1

            
        ";
        public string ConnectionId { get; set; }
        public int ProductCodeId { get; set; }
        public int SampleId { get; set; }
        public int LineId { get; set; }
        public int SystemId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Product { get; set; }
        public string LotNum { get; set; }

        public Context()
        { }

        public Context(string code, string spec)
        {

        }

        public Context(string lot)
        {

        }
    }
}