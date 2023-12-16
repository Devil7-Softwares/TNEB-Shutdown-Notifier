using Quartz;
using TNEB.Shutdown.Notifier.Web.Data;
using TNEB.Shutdown.Notifier.Web.Data.Models;
using TNEB.Shutdown.Scrapper;

namespace TNEB.Shutdown.Notifier.Web.Jobs
{
    public class ScheduleScrapperJob : IJob
    {
        private readonly ILogger<ScheduleScrapperJob> logger;

        public ScheduleScrapperJob(ILogger<ScheduleScrapperJob> logger)
        {
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("ScrapperJob started");
            return Task.CompletedTask;
        }
    }

    public class CircleScrapperJob : IJob
    {
        private readonly ILogger<CircleScrapperJob> logger;
        private readonly AppDbContext dbContext;

        public CircleScrapperJob(ILogger<CircleScrapperJob> logger, AppDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            ICircle[] circles = Array.Empty<ICircle>();

            int addedCircleCount = 0;
            int updatedCircleCount = 0;
            int skippedCircleCount = 0;

            try
            {

                logger.LogDebug("Fetching circles...");
                circles = await Utils.GetCircles();
                logger.LogInformation($"{circles.Length} Circles fetched!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch circles!");
                return;
            }

            if (circles.Length == 0)
            {
                logger.LogWarning("No circles found!");
                return;
            }

            CircleEntry[] circleEntries = Array.Empty<CircleEntry>();

            try
            {
                logger.LogDebug("Fetching existing circles...");
                circleEntries = dbContext.Circles.ToArray();
                logger.LogInformation($"{circleEntries.Length} Circles already exists!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch existing circles!");
                return;
            }

            for (int i = 0; i < circles.Length; i++)
            {
                ICircle circle = circles[i];

                CircleEntry? circleEntry = circleEntries.FirstOrDefault(c => c.Value == circle.Value);

                if (circleEntry == null)
                {
                    logger.LogDebug($"[{i + 1}/{circles.Length}] Adding circle {circle.Name} ({circle.Value})");

                    circleEntry = new CircleEntry
                    {
                        Id = Guid.NewGuid(),
                        Name = circle.Name,
                        Value = circle.Value
                    };

                    dbContext.Circles.Add(circleEntry);

                    addedCircleCount++;
                }
                else if (circleEntry.Name != circle.Name)
                {
                    logger.LogDebug($"[{i + 1}/{circles.Length}] Updating circle {circle.Name} ({circle.Value})");
                    circleEntry.Name = circle.Name;

                    updatedCircleCount++;
                }
                else
                {
                    logger.LogDebug($"[{i + 1}/{circles.Length}] Circle {circle.Name} ({circle.Value}) already exists");
                    skippedCircleCount++;
                }
            }

            try
            {
                if (dbContext.ChangeTracker.HasChanges())
                {
                    logger.LogDebug("Saving circles...");
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation($"Circles saved! Added: {addedCircleCount}, Updated: {updatedCircleCount}, Skipped: {skippedCircleCount}");
                }
                else
                {
                    logger.LogInformation("No circles changed to save!");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save circles!");
            }
        }
    }

    internal static class ScrapperJobExtensions
    {
        public static void ScheduleScrapper(this WebApplication app)
        {
            ISchedulerFactory schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;

            IJobDetail circleScrapperJob = JobBuilder.Create<CircleScrapperJob>().WithIdentity("CircleScrapperJob", "Scrapper").Build();
            ITrigger circleScrapperTrigger = TriggerBuilder.Create()
                .WithIdentity("CircleScrapperTrigger", "Scrapper")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .RepeatForever()
                ).Build();
            scheduler.ScheduleJob(circleScrapperJob, circleScrapperTrigger).Wait();

            IJobDetail scheduleScrapperJob = JobBuilder.Create<ScheduleScrapperJob>().WithIdentity("ScheduleScrapperJob", "Scrapper").Build();
            ITrigger scheduleScrapperTrigger = TriggerBuilder.Create()
                .WithIdentity("ScheduleScrapperTrigger", "Scrapper")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(1)
                    .RepeatForever()
                ).Build();
            scheduler.ScheduleJob(scheduleScrapperJob, scheduleScrapperTrigger).Wait();
        }
    }
}
