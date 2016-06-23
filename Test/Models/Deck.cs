using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using cx = System.Windows.Forms.DataVisualization.Charting;

namespace Test.Models
{
    public partial class Deck
    {
        private static string _deck = @"
        SELECT d.[DeckId]
              ,d.[Name]
          FROM [dbo].[Deck] d
        ";

        private static string _slides = @"
           select
               s.[SlideId] as Slides__SlideId
              ,s.[Name] as Slides__Name
              ,s.[FileNameSuffix] as Slides__FileNameSuffix
              ,s.[FileFormat] as Slides__FileFormat
              ,x.[SeriesId]       as Series__SeriesId
              ,x.[Title]          as Series__Title 
              ,x.[Field]          as Series__Field  
              ,x.[YLabel]         as Series__YLabel  
              ,x.[Legend]         as Series__Legend  
              ,x.[Height]         as Series__Height  
              ,x.[ForeGround]     as Series__ForeGround  
            from [dbo].[Slide] s
            left join [dbo].[SlideSeries] y on s.SlideId = y.SlideId
            left join [dbo].[Series] x on x.SeriesId = y.SeriesId
        ";

        public Size Dimensions { get; set; }
        public List<Slide> Slides { get; set; }

        public Deck() { }

        public Deck(string name)
        {
            IEnumerable<Deck> show = null;
            lastd = null;
            lasts = null;
            Deck d = null;
            using (labDB db = new labDB())
            {
                show = db.Fetch<Deck>(_deck + " where d.[Name] = @0", name);
                d = show.FirstOrDefault();
                if (d == null)
                    return;

                DeckId = d.DeckId;
                Name = d.Name;
                Slides = db.FetchOneToMany<Slide>(s => s.Series, _slides + " where s.[DeckId] = @0", DeckId);
            }
        }

        private static Deck lastd = null;
        private static Slide lasts = null;

        public static Deck Link(Deck d, Slide s, Series x)
        {
            if (x == null)
                return null;

            Deck ret = null;
            if (lastd?.DeckId == d.DeckId) d = lastd;
            if (lastd == null) { lastd = d; ret = lastd; }

            if (d.Slides == null) d.Slides = new List<Slide>();

            if (lasts?.SlideId == s.SlideId)
                s = lasts;
            else
                lasts = s;

            if (!d.Slides.Contains(s))
            {
                s.chart = new cx.Chart();
                d.Slides.Add(s);
            }

            if (s.Series == null) s.Series = new List<Series>();

            if (!s.Series.Contains(x))
            {
                x.core = new cx.Series();
                s.Series.Add(x);
            }

            return ret;
        }
    }

    public partial class Slide
    {
        public cx.Chart chart { get; set; }
        public List<Series> Series { get; set; }
    }

    public partial class Series
    {
        public cx.Series core { get; set; }
        public List<object> values { get; set; }

        public void setvalues<T>(List<T> list)
        {
            var propInfo = list.First().GetType().GetProperty(Field);
            values = list.Select(x => propInfo.GetValue(x, null)).ToList();
        }
    }
}
