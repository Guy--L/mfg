using System;
using System.Collections.Generic;
using System.Linq;

namespace Test.Models
{
    public class LineTimeLine
    {
        public List<LineTime> times;
        public List<LineEvent> events;
        public LineTimeLine(List<Line> list)
        {
            times = list.ConvertAll(l => (LineTime) l);
            events = list.ConvertAll(l => (LineEvent) l);
        }
    }

    public class LineEvent : Line
    {
    }

    public class LineTime : Line
    {
    }

    public partial class LineTx
    {
        private static string _byLine = @"
            select {0}
                [LineTxId],
                [LineId],
                [PersonId],
                [Stamp],
                [Comment],
                [LineTankId],
                [UnitId],
                [LineNumber],
                [SystemId],
                [StatusId],
                [ProductCodeId],
                [ConversionId]
            from linetx where lineid = @0 order by stamp desc
        ";

        private static string _pendingByLine = @"
            select
                [LineId],
                [Scheduled],
                [Completed],
                [Note],
                [SystemId],
                [StatusId],
                [ProductCodeId],
                [ConversionId],
                [SolutionRecipeId]
            from conversion where lineid = @0 order by dbo.SinceNow([Started], [Completed])
        ";

        private static string priorByLine(int id, int horizon)
        {
            var top = horizon > 0 ? ("top " + horizon) : "";
            return string.Format(_byLine, top);
        }

        public static Line Prior(int id)
        {
            List<Line> pair = null;
            using (labDB db = new labDB())
            {
                pair = db.Fetch<Line>(priorByLine(id, 2));
            }
            return pair.Any()? pair.Last(): null; 
        }

        public static List<Line> TimeLine(int id)
        {
            List<Line> past = null;
            List<Line> future = null;

            using (labDB db = new labDB())
            {
                past = db.Fetch<Line>(priorByLine(id, 10));
                future = db.Fetch<Line>(_pendingByLine, id);
                future.ForEach(f => f.Stamp = f.Completed < DateTime.Now ? f.Completed : f.Scheduled);
            }
            var latest = past.Max(p => p.Stamp);
            var timeline = future.Where(f => f.Completed > latest).Concat(past);
            return timeline.ToList();
        }
    }
}