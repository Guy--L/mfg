using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tags.Models
{
    public class PickReview
    {
        public IEnumerable<SelectListItem> graphs { get; set; }
        public string DeletedReviews { get; set; }
        public bool Cancel { get; set; }
        public List<Review> reviews { get; set; }
        public int[] picked { get; set; }
        public int ReviewId { get; set; }
        public string NewReview { get; set; }
        public string NewSchedule { get; set; }
        public string snippet { get; set; }

        public PickReview()
        { }

        public PickReview(int reviewId)
        {
            var gs = Graph.All();
            graphs = gs.Select(p => new SelectListItem { Value = p.GraphId.ToString(), Text = p.GraphName }).ToList();
            reviews = Review.All();

            var r2gs = gs.ToLookup(k => k.ReviewId, v => v.GraphId);
            var ctags = r2gs.Where(k => k.Key != null).Select(r => "charts.push([" + string.Join(",", r.Select(g => g).ToArray()) + "]);");

            snippet = string.Join("\n", ctags.ToArray());
        }
    }
}