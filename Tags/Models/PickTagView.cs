using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Tags.Models
{
    public class PickTagView
    {
        private static string chartbutton = @"
            <button class='btn btn-success btn-lg chartbtn' data-id='{0}'>
                <i class='fa fa-line-chart'></i> {1}
            </button>";

        public bool Cancel { get; set; }
        public string Channel { get; set; }
        public List<Tag> picklist { get; set; }
        public ILookup<string, Series> views { get; set; }
        public string charts { get; set; }
        //public IEnumerable<Tuple<int, int[]>> series { get; set; }
        public string snippet { get; set; }

        public string NewView { get; set; }
        public int[] picked { get; set; }
        public IEnumerable<SelectListItem> tags { get; set; }

        public PickTagView()
        { }

        public PickTagView(string id, string user)
        {
            var picklist = Tag.tagsByLines(id);
            if (picklist.Any())
            {
                Channel = string.Join(", ", picklist.Select(c => c.Channel).Distinct().ToArray());

                var singles = picklist.GroupBy(p => p.Name).Where(g => g.Count() == 1).Select(g => g.First());
                var groups = picklist.Except(singles);
                var lists = groups.Select(q=>q.Name).Distinct().ToDictionary(y => y, y => new SelectListGroup{Name = y});

                tags = picklist.Select(x => lists.ContainsKey(x.Name)? 
                    new SelectListItem
                    { 
                        Value = x.TagId.ToString(), 
                        Text = x.Path.Substring(0,x.Path.Length-x.Name.Length-1), 
                        Group = lists[x.Name]
                    } : new SelectListItem
                    {
                        Value = x.TagId.ToString(),
                        Text = x.Name
                    }
                ).ToList();
            }
            views = Series.seriesByUser(user);
            charts = string.Join("\n",views.Select((v, x) => string.Format(chartbutton, x, v.Key)).ToArray());
            snippet = "";
            if (views.Any())
                snippet = string.Join("\n", "chart.push(["+views.Select(v => string.Join(",",v.Select(y => y.TagId.ToString()).ToArray())).ToString()+"]);");
            //series = views.Select((v, x) => new Tuple<int, int[]>(x, v.Select(y => y.TagId).ToArray()));
            Cancel = false;
        }
    }
}