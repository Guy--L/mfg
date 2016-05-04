using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tags.Jobs;

namespace ChartJobTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");
            var cj = new ChartJob();
            cj.Render(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(-21));
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
