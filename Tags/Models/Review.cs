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
        public static string icon = @"<i class='fa fa-{0}' title='{1}'></i>";
        public int index { get; set; }
        public TimeSpan interval { get; set; }
        public bool running { get; set; }
        public string descriptor { get; set; }
        public List<Graph> graphs { get; set; }

        public bool isEmail { get { return Type == "Mail"; } }

        public string typeicon {
            get {
                if (isEmail) return string.Format(icon, "envelope-o", "scheduled email");
                return string.Format(icon, "file-image-o", "scheduled file creation");
            }
        }

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

        public static Review Save(Review r, bool type, int[] tagids)
        {
            var ids = string.Join(",", tagids);
            using (tagDB t = new tagDB())
            {
                r = t.Single<Review>(";exec dbo.UpdateReview @0, @1, @2, @3, @4, @5"
                        , r.Name
                        , r.Schedule
                        , r.Path
                        , r.Template
                        , type?"Mail":"Chart"
                        , ids);
            }
            return r;
        }
    }
}