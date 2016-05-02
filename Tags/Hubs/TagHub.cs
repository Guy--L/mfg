using Microsoft.AspNet.SignalR;
using Tags.Models;
using Quartz;

namespace Tags.Hubs
{
    public class TagHub : Hub
    {
        public static IScheduler sched;


        public void Weekly()
        {

        }
    }
}