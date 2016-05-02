using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Tags.Models
{
    public class PickTagView
    {
        private static string chartbutton = @"
            <div class='col-md-4'>
                <button class='btn btn-success btn-xs chartbtn view{0}' data-id='{0}' type='submit' title='{3}'>
                    <i class='fa fa-line-chart'></i> {1}
                </button>
                <button class='btn btn-danger btn-xs chartdel view{0}' data-id='{0}' data-graphid='{2}'>
                    <i class='fa fa-remove'></i>
                </button>
                <br />
            </div>
        ";

        public string DeletedViews { get; set; }
        public bool Cancel { get; set; }
        public bool Monitor { get; set; }
        public string Channel { get; set; }
        public List<Tag> picklist { get; set; }
        public ILookup<string, Plot> views { get; set; }
        public bool EditViews { get; set; }
        public string charts { get; set; }
        //public IEnumerable<Tuple<int, int[]>> series { get; set; }
        public string snippet { get; set; }

        public string NewView { get; set; }
        public int[] picked { get; set; }
        public IEnumerable<SelectListItem> tags { get; set; }

        public PickTagView()
        { }

        /// <summary>
        /// Setup tags to pick from.
        /// Provide buttons for prior views that have been saved.
        /// Edit those buttons/views.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        public PickTagView(string id, string user)
        {
            var picklist = Tag.tagsByLines(id);
            charts = "";
            snippet = "";
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

                views = Plot.plotsByUser(user);

                if (views.Any())
                {
                    charts = string.Join("\n", views.Select((v, x) => 
                                    string.Format(chartbutton, x, v.Key, v.First().GraphId,
                                    string.Join("\n", v.Select(t => t.Path.TagPart()))
                                    )).ToArray());

                    var tagsByName = picklist.ToLookup(t => t.Path.TagPart(), v => v.TagId);
                    var ctags = views.Select(view => "charts.push([" + 
                                    string.Join(",", view.Select(plot =>
                                    {
                                        var ids = tagsByName[plot.Path.TagPart()];
                                        var lst = string.Join(",", ids);
                                        return ids.Any() ? lst : null;
                                    })) + "]);");

                    snippet = string.Join("\n", ctags.ToArray());
                }
            }
            Cancel = false;
            Monitor = false;
        }
    }

    public static class PathExtensions
    {
        public static string TagPart(this string path)
        {
            return path.Substring(path.IndexOf('.')+1);
        }
    }
}