using System;
using Tags.Jobs;

namespace ChartJobTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");
            var cj = new ChartJob(2);
            cj.Render(2, DateTime.Now.AddDays(-8), DateTime.Now.AddDays(-1));
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
