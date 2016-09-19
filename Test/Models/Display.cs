using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class Display
    {
        private static string _row = @"
            <div class='row no-gutter'>
                {0}
            </div>
        ";
        private static string _link = @"
            <div class='col-md-4'>
                <a href = '{0}' class='thumbnail'>
                    <img class='img-responsive' src='{1}' />
                </a>
            </div>
        ";
        private static string _down = @"
            <div class='col-md-4'>
                <img class='img-responsive' src='{0}' />
            </div>
        ";

        private List<string> buffer { get; set; }

        public int rows { get; set; }
        public int cols { get; set; }
        public HtmlString Content { get; set; }

        public Display(int r, int c, string template, int[] slides)
        {
            rows = r;
            cols = c;
            buffer = new List<string>(rows);
            for (r = 0; r < rows; r++)
                buffer.Add("");

            int i = 0;
            for (c = 0; c < cols && i < slides.Length; c++)
            {
                for (r = 0; r < rows && i < slides.Length; r++)
                {
                    if (buffer[r] == null) buffer[r] = "";

                    var slide = slides[i++];
                    var path = string.Format(template, slide);
                    var phys = HttpContext.Current.Server.MapPath(path);
                    if (File.Exists(phys))
                    {
                        var thumb = path.Replace(".", "_thumb.");
                        buffer[r] += string.Format(_link, path, thumb);
                        continue;
                    }
                    var newpath = Path.Combine(Path.GetDirectoryName(path), slide + "t" + Path.GetExtension(path));
                    buffer[r] += string.Format(_down, newpath);
                }
            }

            Content = new HtmlString(string.Join("", buffer.Select(s => string.Format(_row, s))));            
        }
    }
}