using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public partial class Review
    {
        public List<Graph> graphs { get; set; }

        public static List<Review> All()
        {
            using (tagDB t = new tagDB())
            {
                return t.Fetch<Review>();
            }
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