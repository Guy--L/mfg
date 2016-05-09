using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CronExpressionDescriptor;

namespace Tags.Models
{
    public partial class Review
    {
        public string descriptor { get; set; }
        public List<Graph> graphs { get; set; }

        public static List<Review> All()
        {
            List<Review> reviews;

            using (tagDB t = new tagDB())
            {
                reviews = t.Fetch<Review>();
            }
            reviews.Any(r => { r.descriptor = ExpressionDescriptor.GetDescription(r.Schedule); return false; });
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