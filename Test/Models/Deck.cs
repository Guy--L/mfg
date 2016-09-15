using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NPoco;
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
               s.[SlideId] 
              ,s.[Name] 
              ,s.[FileNameSuffix] 
              ,s.[FileFormat] 
              ,x.[SeriesId]       as content__SeriesId
              ,x.[Title]          as content__Title 
              ,x.[Field]          as content__Field  
              ,x.[YLabel]         as content__YLabel  
              ,x.[Legend]         as content__Legend  
              ,x.[Height]         as content__Height  
              ,x.[ForeGround]     as content__ForeGround  
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
                Slides = db.FetchOneToMany<Slide>(s => s.content, _slides + " where s.[DeckId] = @0", DeckId);
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

            if (s.content == null) s.content = new List<Series>();

            if (!s.content.Contains(x))
            {
                x.core = new cx.Series();
                s.content.Add(x);
            }

            return ret;
        }
    }

    public partial class Slide
    {
        public cx.Chart chart { get; set; }
        [ResultColumn] public List<Series> content { get; set; }
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
