using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tags.Models
{
    public partial class Group
    {
        private static string _members = Tag._all + @"
            join [TagGroup] m on m.TagId = t.TagId
            join [Group] g on g.GroupId = m.GroupId
        ";

        public static List<Tag> Members(int id)
        {
            List<Tag> m = null;
            using (tagDB t = new tagDB()) {
                m = t.Fetch<Tag>(_members + " where g.GroupId = @0", id);
            }
            return m;
        }

        public static int[] Membership(int id)
        {
            return Members(id).Select(m => m.TagId).ToArray();
        }
    }

    public class GroupView
    {
        private static string _update = @"
            merge taggroup with (holdlock) as t
            using [tag] as s
            on t.groupid = {0} and s.tagid = t.tagid and s.tagid in ({1})
            when not matched by target and s.tagid in ({1}) then
                insert (groupid, tagid) values ({0}, s.tagid)
            when not matched by source and t.groupid = {0} then
                delete;
        ";

        public bool Cancel { get; set; }
        public Group group { get; set; }
        public IEnumerable<SelectListItem> tags { get; set; }
        public int[] picked { get; set; }

        public GroupView() { }

        public GroupView(int id)
        {
            var all = Tag.All();

            if (id != 0)
            {
                group = Group.Single(id);
                picked = Group.Membership(id);
            }
            else
                group = new Group() { GroupId = 0, Name = "", UserId = 0 };

            var singles = all.GroupBy(p => p.Name).Where(g => g.Count() == 1).Select(g => g.First());
            var groups = all.Except(singles);
            var lists = groups.Select(q => q.Name).Distinct().ToDictionary(y => y, y => new SelectListGroup { Name = y });

            tags = all.Select(x => lists.ContainsKey(x.Name) ?
                new SelectListItem
                {
                    Value = x.TagId.ToString(),
                    Text = x.Path.Substring(0, x.Path.Length - x.Name.Length - 1),
                    Group = lists[x.Name],
                    Selected = id==0?false:picked.Contains(x.TagId)
                } : new SelectListItem
                {
                    Value = x.TagId.ToString(),
                    Text = x.Name,
                    Selected = id==0?false:picked.Contains(x.TagId)
                }
            ).ToList();
        }

        public void Save()
        {
            group.Save();
            using (tagDB t = new tagDB())
            {
                t.Execute(string.Format(_update, group.GroupId, string.Join(",", picked)));
            }
        }
    }
}