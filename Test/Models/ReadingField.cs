using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Test.Models
{
    public partial class ReadingField
    {
        public PropertyInfo propInfo { get; set; }

        public void reflect(object parent)
        {
            propInfo = parent.GetType().GetProperty(FieldName);
        }
    }
}