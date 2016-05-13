using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CronExpressionDescriptor;
using Quartz;
using Quartz.Impl.Matchers;
using Tags.Hubs;

namespace Tags.Models
{
    public partial class Review
    {
        public int index { get; set; }
        public TimeSpan interval { get; set; }
        public bool running { get; set; }
        public string descriptor { get; set; }
        public List<Graph> graphs { get; set; }

        public Review() { }

        public static List<Review> All()
        {
            List<Review> reviews;

            using (tagDB t = new tagDB())
            {
                reviews = t.Fetch<Review>();
            }
            TagHub.JobList(reviews);
            return reviews;
        }

        public static void Save(string name, string cron, int[] tagids)
        {
            var ids = string.Join(",", tagids);
            using (tagDB t = new tagDB())
            {
                t.Execute(";exec dbo.UpdateReview @0, @1, @2", name, cron, ids);
            }
        }
    }
}