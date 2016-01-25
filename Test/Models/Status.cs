using System.Collections.Generic;
using System.Linq;

namespace Test.Models
{
    public partial class Status
    {
        public static Dictionary<int, string> legend;

        public static void SetIcons(List<Status> s)
        {
            legend = s.ToDictionary(k => k.StatusId, n => n.Iconic());
        }

        public string Pretty(bool isActive)
        {
            return "<i class='fa fa-2x fa-" + (isActive?"check-":"") + "square'></i> &nbsp;" + Description + " <i class='fa fa-" + Icon + "'></i>";
        }

        public string Iconic()
        {
            return "<i class='fa fa-" + Icon + "' title='" + Description + "'></i>";
        }
    }
}