using System;
using Elmah;
using Microsoft.AspNet.SignalR;
using Quartz;
using Tags.Hubs;
using Tags.Models;

namespace Tags.Jobs
{
    public abstract class JobBase : IJob
    {
        public int jobid = 0;
        public Review review = null;

        protected JobBase()
        {
        }

        public abstract void ExecuteJob(IJobExecutionContext context);

        public void Execute(IJobExecutionContext context)
        {
            string logSource = null;
            try
            {
                logSource = context.JobDetail.Key.ToString();
                jobid = int.Parse(context.JobDetail.Key.Name);
                review = Review.SingleOrDefault(jobid);

                ExecuteJob(context);

                var hub = GlobalHost.ConnectionManager.GetHubContext<TagHub>();
                hub.Clients.All.updateTime(jobid, DateTime.Now.ToString("MM/dd HH:mm"));

                review.LastRun = DateTime.Now;
                review.Update();
            }
            catch (Exception e)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Error(new Exception(logSource, e)));
            }
        }
    }
}