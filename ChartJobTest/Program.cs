using System;
using Tags.Jobs;

namespace ChartJobTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");
            var cj = new ChartJob();
            cj.Render(1, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-2));
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
