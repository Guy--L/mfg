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
            cj.Render(2, DateTime.Now.AddDays(-7), DateTime.Now);
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
