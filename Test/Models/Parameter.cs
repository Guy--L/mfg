using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Test.Models
{
    public partial class Parameter
    {
        public string TypeName
        {
            get
            {
                return Regex.Replace(Name, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
            }
        }
    }
}