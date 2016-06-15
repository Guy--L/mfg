﻿using System.Linq;
using System.Collections.Generic;
using System;
using NPoco;
using NPoco.Expressions;
using NPoco.Linq;

namespace Tags.Models
{
    public partial class Graph
    {
        [Reference(ReferenceType.Many, ColumnName = "GraphId", ReferenceMemberName = "GraphId")]
        [Column] public List<Plot> Plots { get; set; }

        private static string _graphsByUser = @"
            select g.graphname
                ,g.shared
                ,p.plotid
                ,p.tagid
                from graph g
                join plot p on g.graphid = p.plotid
                where g.userid = @0
        ";

        private static string _deleteView = @"
            delete p
            from plot p
            where p.graphid in ({0})

            delete g 
            from graph g 
            where g.graphid in ({0})
        ";

        private static string _plotsByReview = @"
            select 
            g.GraphId,
            g.GraphName,
            g.UserId,
            g.Shared,
            g.ReviewId,
            g.MinY,
            g.MaxY,
            p.PlotId as Plots__PlotId,
            p.GraphId as Plots__GraphId,
            p.TagId as Plots__TagId,
            p.YAxis as Plots__YAxis,
            p.Relabel as Plots__Relabel,
            p.Scale as Plots__Scale,
            p.MinY as Plots__MinY,
            p.MaxY as Plots__MaxY
            from [Graph] g
            inner join [Plot] p on p.GraphId = g.GraphId
            where g.ReviewId = @0
        ";

        private static string _tagsByGraph = @"
            select
                c.Name+'.'+d.Name+'.'+t.Name as [Path],
                t.TagId,
                t.DeviceId,
                t.Name,
                t.Address,
                t.DataType,
                c.Name as [Channel],
                d.Name as [Device],
                p.TagId as [CanonId]
            from Tag t
            join Device d on t.DeviceId = d.DeviceId
			join Channel c on c.ChannelId = d.ChannelId
            join Tag tt on t.Name = tt.Name
            join Device dd on d.Name = dd.Name and tt.DeviceId = dd.DeviceId
            join plot p on p.tagid = tt.tagid
            where p.GraphId = @0
            order by p.plotid
        ";

        public DateTime start { get; set; }
        public DateTime end { get; set; }

        public static List<Graph> All()
        {
            using (tagDB t = new tagDB())
            {
                return t.Fetch<Graph>();
            }
        }

        public static void DeleteView(string ids)
        {
            using (tagDB t = new tagDB())
            {
                t.Execute(string.Format(_deleteView, ids));
            }
        }

        public static void SaveView(User user, string name, int[] tagids)
        {
            var ids = string.Join(",", tagids);
            using (tagDB t = new tagDB())
            {
                t.Execute(";exec dbo.UpdatePlot @0, @1, @2, @3", user.Login, user.Identity, name, ids);
            }
        }

        public static List<Graph> plotsByReview(int reviewId)
        {
            List<Graph> results;

            using (tagDB tdb = new tagDB())
            {
                results = tdb.FetchOneToMany<Graph>(g => g.Plots, _plotsByReview, reviewId);
            }
            return results;
        }

        public List<string> Canon()
        {
            List<Tag> tags;
            using (tagDB t = new tagDB())
            {
                tags = t.Fetch<Tag>(_tagsByGraph, GraphId);
            }
            Plots.Select(p => { p.tags = tags.Where(q => q.CanonId == p.TagId).ToList(); return 1; }).ToList();
            
            return tags.Select(c => c.Channel).Distinct().ToList();
        }
    }
}