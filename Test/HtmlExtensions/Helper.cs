using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Test.HtmlExtensions
{
    public static class Helper
    {
        public static string RequireScript(this HtmlHelper html, string path, int priority = 1)
        {
            var requiredScripts = HttpContext.Current.Items["RequiredScripts"] as List<ResourceInclude>;
            if (requiredScripts == null) HttpContext.Current.Items["RequiredScripts"] = requiredScripts = new List<ResourceInclude>();
            if (!requiredScripts.Any(i => i.Path == path)) requiredScripts.Add(new ResourceInclude() { Path = path, Priority = priority });
            return null;
        }

        public static HtmlString EmitRequiredScripts(this HtmlHelper html)
        {
            var requiredScripts = HttpContext.Current.Items["RequiredScripts"] as List<ResourceInclude>;
            if (requiredScripts == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (var item in requiredScripts.OrderByDescending(i => i.Priority))
            {
                sb.AppendFormat("<script src=\"{0}\" type=\"text/javascript\"></script>\n", item.Path);
            }
            return new HtmlString(sb.ToString());
        }

        private static string _downloadXLS = @"
            <form action='/Home/GetSpreadSheet' class='form-inline' id='zoomxlsform' method='post'>   
                <li class='download'>
                    <input id = 'Product' name='Product' type='hidden' value='' />
                    <input id = 'Start' name='Start' type='hidden' value='' />
                    <input id = 'End' name='End' type='hidden' value='' />
                    <button class='btn btn-xs btn-success' type='submit' id='getdetail'><i class='fa fa-cloud-download'></i>&nbsp;xlsx</button>
                </li>
            </form>
        ";

        public static string RequireXLSDownload(this HtmlHelper html)
        {
            HttpContext.Current.Items["XLSDownload"] = true;
            html.RequireScript("/Scripts/_xlsdownload.js");
            return null;
        }

        public static HtmlString EmitXLSDownloadOption(this HtmlHelper html)
        {
            var downloadXLS = HttpContext.Current.Items["XLSDownload"] as bool?;
            if (downloadXLS == null) return null;
            return new HtmlString(_downloadXLS);
        }

        public class ResourceInclude
        {
            public string Path { get; set; }
            public int Priority { get; set; }
        }
    }
}