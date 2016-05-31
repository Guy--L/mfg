using System;
using Postal;
using Quartz;

namespace Tags.Jobs
{
    public class MailJob : JobBase
    {
        public override void ExecuteJob(IJobExecutionContext context)
        {
            dynamic email = new Email("Test");
            email.To = review.Template;
            email.Job = context.JobDetail.Key.ToString();
            email.Time = DateTime.Now.ToShortTimeString();
            email.Send();
        }
    }
}