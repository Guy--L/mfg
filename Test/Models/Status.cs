using System.Collections.Generic;
using System.Linq;

namespace Test.Models
{
    public partial class Status
    {
        public static Dictionary<string, int> statuses;

        public static Dictionary<int, Status> state;

        public static void SetIcons(List<Status> s)
        {
            statuses = s.ToDictionary(k => k.Description, v => v.StatusId);
            state = s.ToDictionary(k => k.StatusId, v => v);
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