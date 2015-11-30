using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Test.Models
{
    public class PickFieldView
    {
        public bool Cancel { get; set; }
        public string Record { get; set; }
        public List<string> picklist { get; set; }

        public string newview { get; set; }
        public int[] picked { get; set; }
        public IEnumerable<SelectListItem> tags { get; set; }

        public PickFieldView()
        { }

        public PickFieldView(List<PropertyInfo> cols)
        {
            //var p = cols.Select((i, c)=> new {idx = i, val = c }).Select(q => new SelectListItem({ Value = q.idx, Text = Que}));
           // var p = cols.Select((i, c)=> new {idx = i, val = c }).Select(q => new SelectListItem({ Value = q.idx, Text = Que}));
        }

        public PickFieldView(Type t)
        {
            Record = t.Name;
            var picklist = t.GetProperties().Select(p => p.Name);
            if (picklist.Any())
            {
                tags = picklist.Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                });
            }
            Cancel = false;
        }
    }
}