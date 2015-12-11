using System.Web;

namespace Test.Models
{
    public partial class Status
    {
        public string Pretty(bool isActive)
        {
            return "<i class='fa fa-2x fa-" + (isActive?"check-":"") + "square'></i> &nbsp;" + Description + " <i class='fa fa-" + Icon + "'></i>";
        }
    }
}