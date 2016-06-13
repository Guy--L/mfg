using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using ReadPlans.Models;

namespace ReadPlans
{
    class Program
    {
        static int count = 0;

        static void Main(string[] args)
        {
            var inputdir = ConfigurationManager.AppSettings["input"];

            count = 0;
            foreach (var year in args)
            {
                var input = Path.Combine(inputdir, year);

                DirectoryInfo info = new DirectoryInfo(input);
                var filesin = info.GetFiles();
                var names = filesin.Select(f => f.Name.Replace("Schedule", "").Replace("Sched", "").Trim()).ToLookup(k => k.Substring(0,6));
                var finals = names.Select(s => s.Reverse().Skip(s.Count() > 1 ? 1 : 0).Take(1).Single()).ToList();

                foreach (var file in finals)
                {
                    count++;
                    Plan.GetPlans(Path.Combine(input, "Schedule "+file));
                    //Console.Write("\r{0} read, {1} weeks         ", file, count);
                }
            }
            Plan.Stats();
            Console.ReadKey();
        }

    }
}
