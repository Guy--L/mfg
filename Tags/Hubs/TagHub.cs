using Microsoft.AspNet.SignalR;
using Tags.Models;
using Quartz;
using Tags.Jobs;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Quartz.Impl.Matchers;
using System.Linq;
using CronExpressionDescriptor;

namespace Tags.Hubs
{
    public class TagHub : Hub
    {
        public static IScheduler sched;

        public static void Update(Review r)
        {
            var job = new JobKey(r.ReviewId.ToString(), r.Name);
            var triggers = sched.GetTriggersOfJob(job);

            var otrigger = triggers.FirstOrDefault() as ICronTrigger;
            if (otrigger != null && otrigger.CronExpressionString != r.Schedule)
            {
                ITrigger ntrigger = TriggerBuilder.Create()
                    .ForJob(job)
                    .WithIdentity(r.ReviewId.ToString(), r.Name)
                    .StartNow()
                    .WithCronSchedule(r.Schedule)
                    .Build();

                sched.RescheduleJob(otrigger.Key, ntrigger);
            }
        }

        public static void JobList(List<Review> reviews)
        {
            var jobGroups = sched.GetJobGroupNames();
            reviews.Select((r, i) =>
            {
                r.index = i;
                r.running = false;
                r.descriptor = string.IsNullOrWhiteSpace(r.Schedule) ? "" : ExpressionDescriptor.GetDescription(r.Schedule);
                return false;
            }).ToList();

            foreach (var group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = sched.GetJobKeys(groupMatcher);

                var review = reviews.SingleOrDefault(r => r.Name == group);
                if (review != null)
                    review.running = true;

                foreach (var jobKey in jobKeys)
                {
                    var detail = sched.GetJobDetail(jobKey);
                    var triggers = sched.GetTriggersOfJob(jobKey);

                    foreach (ITrigger trigger in triggers)
                    {
                        var simpleTrigger = trigger as ISimpleTrigger;
                        var previousFireTime = trigger.GetPreviousFireTimeUtc();
                        if (review == null)
                        {
                            review = new Review() { ReviewId = -int.Parse(jobKey.Name), Name = group, running = true, LastRun = DateTime.MinValue };
                            reviews.Add(review);
                        }
                        if (previousFireTime.HasValue && previousFireTime.Value.LocalDateTime > review.LastRun)
                        {
                            review.LastRun = previousFireTime.Value.LocalDateTime;
                            if (simpleTrigger != null) review.interval = simpleTrigger.RepeatInterval;
                        }
                    }
                }
            }
        }

        public void StartJob(int reviewId)
        {
            var r = Review.Single(reviewId);

            var type = typeof(IJob);

            var jobtype = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .SingleOrDefault(p => type.IsAssignableFrom(p) && p.FullName.Contains(r.Type) && p.FullName.StartsWith("Tags.Jobs"));

            if (jobtype == null)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new NotImplementedException("Could not find implementation for job of type " + r.Type + "Job in StartJob"));
                return;
            }

            try {
                IJobDetail job = JobBuilder.Create(jobtype)
                    .WithIdentity(r.ReviewId.ToString(), r.Name)
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .ForJob(job)
                    .WithIdentity(r.ReviewId.ToString(), r.Name)
                    .StartNow()
                    .WithCronSchedule(r.Schedule)
                    .Build();

                sched.ScheduleJob(job, trigger);
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("trying to start job", e));
            }
        }

        public Review StopJob(int reviewId, string name)
        {
            var r = Review.SingleOrDefault(reviewId);
            reviewId = reviewId < 0 ? -reviewId : reviewId;
            var j = new JobKey(reviewId.ToString(), name);
            sched.DeleteJob(j);
            return r;
        }

        public void DeleteJob(int reviewId, string name)
        {
            var r = StopJob(reviewId, name);
            if (r!=null) Review.Delete(r);
        }
    }
}