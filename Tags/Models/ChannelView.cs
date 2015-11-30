using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public class ChannelView
    {
        private static string _units = @"ABCD";

        public List<Channel> nogrid { get; set; }
        public Dictionary<char,int[]> grid { get; set; } 

        public ChannelView(List<Channel> ch)
        {
            grid = new Dictionary<char, int[]>();
            nogrid = new List<Channel>();
            foreach (var c in ch)
            {
                var i = _units.IndexOf(c.Name[0]);

                if (i < 0 || !char.IsDigit(c.Name[1])) {
                    nogrid.Add(c);
                    continue;
                }
                if (!grid.ContainsKey(c.Name[0]))
                {
                    grid[c.Name[0]] = new int[8];
                }

                grid[c.Name[0]][c.Name[1] - '1'] = c.ChannelId;
            }
        }
    }
}