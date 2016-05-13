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

        public static void Update(int reviewId, string reviewName, string schedule)
        {
            var job = new JobKey(reviewId.ToString(), reviewName);
            var triggers = sched.GetTriggersOfJob(job);

            var otrigger = triggers.FirstOrDefault() as ICronTrigger;
            if (otrigger != null && otrigger.CronExpressionString != schedule)
            {
                ITrigger ntrigger = TriggerBuilder.Create()
                    .ForJob(job)
                    .WithIdentity(reviewId.ToString(), reviewName)
                    .StartNow()
                    .WithCronSchedule(schedule)
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
                var jobKeys = TagHub.sched.GetJobKeys(groupMatcher);

                var review = reviews.SingleOrDefault(r => r.Name == group);
                if (review == null)
                    continue;

                review.running = true;
                foreach (var jobKey in jobKeys)
                {
                    var detail = TagHub.sched.GetJobDetail(jobKey);
                    var triggers = TagHub.sched.GetTriggersOfJob(jobKey);

                    foreach (ITrigger trigger in triggers)
                    {
                        var simpleTrigger = trigger as ISimpleTrigger;
                        var previousFireTime = trigger.GetPreviousFireTimeUtc();
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

            try {
                IJobDetail job = JobBuilder.Create<ChartJob>()
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
                Debug.WriteLine("trigger failed");
            }
        }

        public Review StopJob(int reviewId)
        {
            var r = Review.Single(reviewId);
            var j = new JobKey(reviewId.ToString(), r.Name);
            sched.DeleteJob(j);
            return r;
        }

        public void DeleteJob(int reviewId)
        {
            var r = StopJob(reviewId);
            Review.Delete(r);
        }
    }
}