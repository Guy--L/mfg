using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace TagService
{
    public class GroupName
    {
        public string Value { get; set; }
    }

    public static class TagCache
    {
        public static string _suffix = ".Value";
        private static string _server = ConfigurationManager.AppSettings["TagServer"];
        private static string _prefix = @"\\" + _server;

        private static OPCControlsData _data = new OPCControlsData();
        private static OPCSystemsComponent _source = new OPCSystemsComponent();

        private static Dictionary<string, List<string>> _groups = new Dictionary<string, List<string>>();          // group to tags
        private static Dictionary<string, List<GroupName>> _tags = new Dictionary<string, List<GroupName>>();      // tag to groups

        private static Dictionary<string, int> _groupcount = new Dictionary<string, int>();
        private static Dictionary<string, Dictionary<string, object>> _values = new Dictionary<string, Dictionary<string, object>>();

        private static IHubContext _context = GlobalHost.ConnectionManager.GetHubContext<TagHub>();

        public static void Subscribe()
        {
            //List<string> wet = new List<string>(wetmain);
            //var tags = wet.Select(t => prefix + t + suffix);
            //wetTags = tags.ToArray();
            //data.AddTags(wetTags);

            //_context = GlobalHost.ConnectionManager.GetHubContext<TagHub>();
            //data.ValuesChangedAll += data_ValuesChangedAll;
            //Debug.WriteLine("Values changed event handler in");
        }

        public static void Write(string id, object value)
        {
            _data.WriteTag(id.Replace("xpct", "%"), value);
        }

        public static ClassTagValues ReadAll(string group)
        {
            var gTags = _values[group];
            return new ClassTagValues(gTags.Keys.ToArray(), gTags.Values.ToArray());
        }

        public static void LocalSubscribe(List<string> add)
        {
            //string[] tags = add.ToArray();
            //data.AddTags(tags);
            //if (_context == null)
            //{
            //    _context = GlobalHost.ConnectionManager.GetHubContext<TagHub>();
            //    data.ValuesChangedAll += data_ValuesChangedAll;
            //}
        }

        /// <summary>
        /// map from tag name to group.
        /// 
        /// return list of dictionary entries that already exist for those tags that are already in other groups
        /// </summary>
        /// <param name="group"></param>
        /// <param name="add"></param>
        private static void inverseMap(string group, IEnumerable<string> add)
        {
            var newGroup = new GroupName { Value = group };         // reference for tag to group lookup

            foreach (var t in add)
            {
                if (!_tags.ContainsKey(t))                            // tag is nowhere else in inverseMap
                    _tags.Add(t, new List<GroupName> { newGroup });  // add it with boxed group name
                else if (!_tags[t].Any(g => g.Value == group))        // tag exists already, does this group?
                    _tags[t].Add(newGroup);                          // add group if not
            }
        }

        private static string[] inverseUnmap(string group)
        {
            if (!_values.ContainsKey(group))                            // group exists?
                return null;

            string tmpl = _values[group]["X"].ToString();
            var tag1 = string.Format(tmpl, _values[group].Keys.First());
            var deadGroup = _tags[tag1].First(g => g.Value == group);   // get first tag in group to get boxed group
            List<string> only = new List<string>();
            foreach (var t in _values[group].Keys)                      // forall tags in group
            {
                if (t == "X" || t.Contains("xpct")) continue;
                var tag2 = string.Format(tmpl, t);
                _tags[tag2].Remove(deadGroup);                          // remove box from inverse map
                if (_tags[tag2].Count() > 0) continue;                  // if tag was only in this group
                only.Add(tag2);                                         // add to return list of tags only in this group
                _tags.Remove(tag2);                                     // remove inverse map entry
            }
            _values.Remove(group);
            return only.ToArray();
        }

        public static string Subscribe(string group, List<string> add)
        {
            // get the unique group names out of this list of tags.  Consider using node structure on kepware server to dictate this.

            if (_values.ContainsKey(group))
            {
                //_groupcount[group]++;      this was moved to the Subscribed test due to overlapping threads during a page refresh
                return "";
            }
            _groupcount.Add(group, 1);
            inverseMap(group, add);

            _data.AddTags(add.ToArray());
            Debug.WriteLine(add.Count + " _tags added for group " + group);
            Debug.WriteLine("added tags:");
            Debug.WriteLine("\t" + string.Join("\n\t", add));


            var parts = add.Select(a => a.Substring(_prefix.Length + 1, a.Length - _prefix.Length - _suffix.Length - 1).Split('.'));
            var newGroups = parts.Select(p => string.Join(".", p.Take(p.Count() - 1))).Distinct();

            Debug.WriteLine("newGroups:\n" + string.Join("\n", newGroups.ToArray()));
            var tgs = parts.Select(p => string.Join(".", p.Skip(p.Count() - 3)));

            string errors = "";

            var tagnValues = newGroups.SelectMany(g => _source.GetTagValuesByGroup(g, _server, ref errors));
            var tagParts = tagnValues.Where((v, k) => k % 2 == 0).Select(v => v.ToString().Split('.'));
            var tagNames = tagParts.Select(p => string.Join(".", p.Skip(p.Count() - 3)));
            var tagValues = tagnValues.Where((v, k) => k % 2 != 0);
            var gvalues = tagNames.Zip(tagValues, (n, v) => new { n, v }).Where(z => tgs.Contains(z.n)).ToDictionary(x => x.n, x => x.v);
            gvalues.Add("X", add.First().Replace(tgs.First(), "{0}"));
            _values.Add(group, gvalues);

            if (_values.Count() == 1)
            {
                _data.ValuesChangedAll += data_ValuesChangedAll;
                Debug.WriteLine("event hooked");
            }

            return errors;
        }

        public static void TestSubscribe(string group, List<string> add)
        {
            // get the unique group names out of this list of tags.  Consider using node structure on kepware server to dictate this.

            if (_values.ContainsKey(group))
            {
                //_groupcount[group]++;                     incremented in Subscribed instead because page refresh overlaps old with new
                Debug.WriteLine(group + " already done");
                return;
            }
            _groupcount.Add(group, 1);
            inverseMap(group, add);

            _data.AddTags(add.ToArray());
            Debug.WriteLine(add.Count + " tags added");
            Debug.WriteLine("\t" + string.Join("\n\t", add.ToArray()));

            var tgs = add.Select(a => a.Substring(0, a.Length - _suffix.Length).Split('\\').Last());

            //var test = source.GetTag_Parameter_Values("Value", tags.First());

            var gvalues = tgs.ToDictionary(x => x, x => _source.GetTag_Parameter_Values("Value", x)[2]);
            gvalues.Add("X", add.First().Replace(tgs.First(), "{0}"));

            _values.Add(group, gvalues);
            if (_values.Count() == 1)
            {
                _data.ValuesChangedAll += data_ValuesChangedAll;
                Debug.WriteLine("event hooked");
            }
        }

        public static bool Subscribed(string group)
        {
            var subscribed = _values.ContainsKey(group);
            if (subscribed) _groupcount[group]++;
            return subscribed;
        }

        static void data_ValuesChangedAll(string[] Tags, object[] Values, bool[] Qualities, DateTime[] TimeStamps)
        {
            foreach (var group in Tags.SelectMany(t => _tags[t]).Distinct())
            {
                if (!_values.ContainsKey(group.Value))
                    continue;
                var dict = _values[group.Value];
                var ctv = new ClassTagValues(dict.Keys.ToList(), Tags.Select(t => t.Split('\\').Last()).ToArray(), Values);
                ctv.Update(dict);                                                       // update cached values
                Debug.WriteLine(group.Value + ":");
                Debug.WriteLine("\t" + string.Join("\n\t", ctv.TagNames.Take(5).ToArray()));
                Debug.WriteLine("\t" + string.Join("\n\t", ctv.Values.ToArray()));
                _context.Clients.Group(group.Value).tagUpdate(ctv);                     // publish changed values to group
            }
        }

        public static void UnSubscribe(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                return;

            _groupcount[group]--;
            if (_groupcount[group] > 0)
                return;

            _groupcount.Remove(group);
            var deadtags = inverseUnmap(group);
            _data.RemoveTags(deadtags);
            Debug.WriteLine("removed tags:");
            Debug.WriteLine("\t" + string.Join("\n\t", deadtags));
            if (_groupcount.Count() == 0)
            {
                _data.ValuesChangedAll -= data_ValuesChangedAll;
                Debug.WriteLine("event unhooked");
            }
        }
    }

    public class ClassTagValues
    {
        public string[] TagNames;
        public object[] Values;
        //        public bool[] Qualities;
        //        public System.DateTime[] TimeStamps;

        public void Update(Dictionary<string, object> store)
        {
            for (int i = 0; i < TagNames.Length; i++)
            {
                store[TagNames[i]] = Values[i];
                TagNames[i] = TagNames[i].Split('.').Last();
            }
        }

        public ClassTagValues(string[] NewTagNames, object[] NewValues)
        {
            TagNames = NewTagNames.Select(s => s.Replace("%", "xpct")).ToArray();
            Values = NewValues;
            //Qualities = NewQualities;
            //TimeStamps = NewTimeStamps;
        }

        public ClassTagValues(List<string> dict, string[] NewTagNames, object[] NewValues)
        {
            var nosuffix = NewTagNames.Select(s => { var p = s.Split('.'); return string.Join(".", p.Skip(p.Length - 4).Take(3).ToArray()); }).ToArray();
            TagNames = nosuffix.Where(n => dict.Contains(n)).Select(s => s.Replace("%", "xpct")).ToArray();
            Values = NewValues.Where((n, i) => dict.Contains(nosuffix[i])).ToArray();
        }
    }
}
