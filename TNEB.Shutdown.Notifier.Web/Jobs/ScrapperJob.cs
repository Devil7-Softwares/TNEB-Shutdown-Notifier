using Quartz;
using TNEB.Shutdown.Notifier.Web.Data;
using TNEB.Shutdown.Notifier.Web.Data.Models;
using TNEB.Shutdown.Scrapper;

namespace TNEB.Shutdown.Notifier.Web.Jobs
{
    public class ScheduleScrapperJob(ILogger<ScheduleScrapperJob> logger, AppDbContext dbContext) : IJob
    {
        public static readonly JobKey Key = new("ScheduleScrapperJob", "Scrapper");

        private readonly ILogger<ScheduleScrapperJob> logger = logger;
        private readonly AppDbContext dbContext = dbContext;

        private Location[]? locations;

        private async Task<Location> GetLocation(string locationName)
        {
            if (locations == null)
            {
                logger.LogDebug("Fetching locations...");
                locations = [.. dbContext.Locations];
                logger.LogInformation("{LocationCount} Locations fetched!", locations.Length);
            }

            Location? location = locations.FirstOrDefault(l => l.Name == locationName);

            if (location == null)
            {
                LocationStandardization? locationStandardization = dbContext.LocationStandardization.FirstOrDefault(l => l.Location == locationName);

                if (locationStandardization != null)
                {
                    location = dbContext.Locations.FirstOrDefault(l => l.Name == locationStandardization.StandardizedLocation);
                }

                if (location == null)
                {
                    logger.LogDebug("Adding location {LocationName}...", locationName);

                    location = new Location
                    {
                        Id = Guid.NewGuid(),
                        Name = locationStandardization?.StandardizedLocation ?? locationName
                    };

                    dbContext.Locations.Add(location);

                    await dbContext.SaveChangesAsync<Location>();
                    logger.LogDebug("Location {LocationName} added!", locationName);
                }
            }

            return location;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Circle[] circles = [];

            try
            {
                logger.LogDebug("Fetching circles for fetching schedules...");
                circles = [.. dbContext.Circles];
                logger.LogInformation("{CirclesLength} Circles fetched for fetching schedules!", circles.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch circles for fetching schedules!");
                return;
            }

            if (circles.Length == 0)
            {
                logger.LogWarning("No circles found for fetching schedules!");
                return;
            }

            Dictionary<Circle, ISchedule[]> circleSchedule = [];

            for (int i = 0; i < circles.Length; i++)
            {
                Circle circleEntry = circles[i];
                logger.LogDebug("[{CurrentIndex}/{CirclesLength}] Fetching schedules for circle {CircleEntryName} ({CircleEntryValue})...", i + 1, circles.Length, circleEntry.Name, circleEntry.Value);

                try
                {
                    ISchedule[] circleSchedules = await Scrapper.Utils.GetSchedules(circleEntry.Value);
                    logger.LogInformation("[{CurrentIndex}/{CirclesLength}] {CircleSchedulesLength} Schedules fetched for circle {CircleEntryName} ({CircleEntryValue})!", i + 1, circles.Length, circleSchedules.Length, circleEntry.Name, circleEntry.Value);

                    circleSchedule.Add(circleEntry, circleSchedules);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{CurrentIndex}/{CirclesLength}] Failed to fetch schedules for circle {CircleEntryName} ({CircleEntryValue})!", i + 1, circles.Length, circleEntry.Name, circleEntry.Value);
                    continue;
                }
            }

            foreach (KeyValuePair<Circle, ISchedule[]> keyValue in circleSchedule)
            {
                Data.Models.Circle circle = keyValue.Key;
                ISchedule[] schedules = keyValue.Value;

                foreach (ISchedule schedule in schedules)
                {
                    string[] locationNames = schedule.Location.Split(',').Select(l => l.Trim()).TakeWhile(l => !string.IsNullOrWhiteSpace(l)).ToArray();

                    foreach (string locationName in locationNames)
                    {
                        Location location = await GetLocation(locationName);

                        Data.Models.Schedule? existingScheduleEntry = dbContext.Schedules.FirstOrDefault(s => s.Date == schedule.Date && s.From == schedule.From && s.To == schedule.To && s.LocationId == location.Id && s.CircleId == circle.Id);

                        if (existingScheduleEntry == null)
                        {
                            logger.LogDebug("Adding schedule for {LocationName} ({ScheduleDate})...", location.Name, schedule.Date.ToString("yyyy-MM-dd"));

                            Schedule scheduleEntry = new()
                            {
                                Id = Guid.NewGuid(),
                                Date = schedule.Date,
                                From = schedule.From,
                                To = schedule.To,
                                Town = schedule.Town,
                                SubStation = schedule.SubStation,
                                Feeder = schedule.Feeder,
                                TypeOfWork = schedule.TypeOfWork,
                                LocationId = location.Id,
                                CircleId = circle.Id
                            };

                            dbContext.Schedules.Add(scheduleEntry);

                            logger.LogInformation("Schedule for {LocationName} ({LocationId}) added!", location.Name, location.Id);
                        }
                        else
                        {
                            logger.LogDebug("Schedule for {LocationName} ({LocationId}) already exists!", location.Name, location.Id);
                        }
                    }
                }
            }

            try
            {
                if (dbContext.ChangeTracker.HasChanges())
                {
                    logger.LogDebug("Saving schedules...");
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Schedules saved!");
                }
                else
                {
                    logger.LogInformation("No schedules changed to save!");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save schedules!");
            }
        }
    }

    public class CircleScrapperJob(ILogger<CircleScrapperJob> logger, AppDbContext dbContext) : IJob
    {
        public static readonly JobKey Key = new("CircleScrapperJob", "Scrapper");

        private readonly ILogger<CircleScrapperJob> logger = logger;
        private readonly AppDbContext dbContext = dbContext;

        public async Task Execute(IJobExecutionContext context)
        {
            ICircle[] circles = [];

            int addedCircleCount = 0;
            int updatedCircleCount = 0;
            int skippedCircleCount = 0;

            try
            {

                logger.LogDebug("Fetching circles...");
                circles = await Scrapper.Utils.GetCircles();
                logger.LogInformation("{CirclesLength} Circles fetched!", circles.Length);
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

            Circle[] circleEntries = [];

            try
            {
                logger.LogDebug("Fetching existing circles...");
                circleEntries = [.. dbContext.Circles];
                logger.LogInformation("{CircleEntriesLength} Circles already exists!", circleEntries.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch existing circles!");
                return;
            }

            for (int i = 0; i < circles.Length; i++)
            {
                ICircle circle = circles[i];

                Circle? circleEntry = circleEntries.FirstOrDefault(c => c.Value == circle.Value);

                if (circleEntry == null)
                {
                    logger.LogDebug("[{CurrentIndex}/{CirclesLength}] Adding circle {CircleName} ({CircleValue})", i + 1, circles.Length, circle.Name, circle.Value);

                    circleEntry = new()
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
                    logger.LogDebug("[{CurrentIndex}/{CirclesLength}] Updating circle {CircleName} ({CircleValue})", i + 1, circles.Length, circle.Name, circle.Value);
                    circleEntry.Name = circle.Name;

                    updatedCircleCount++;
                }
                else
                {
                    logger.LogDebug("[{CurrentIndex}/{CirclesLength}] Circle {CircleName} ({CircleValue}) already exists", i + 1, circles.Length, circle.Name, circle.Value);
                    skippedCircleCount++;
                }
            }

            try
            {
                if (dbContext.ChangeTracker.HasChanges())
                {
                    logger.LogDebug("Saving circles...");
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Circles saved! Added: {AddedCircleCount}, Updated: {UpdatedCircleCount}, Skipped: {SkippedCircleCount}", addedCircleCount, updatedCircleCount, skippedCircleCount);
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

            IJobDetail circleScrapperJob = JobBuilder.Create<CircleScrapperJob>().WithIdentity(CircleScrapperJob.Key).Build();

            if (!scheduler.CheckExists(circleScrapperJob.Key).Result)
            {
                ITrigger circleScrapperTrigger = TriggerBuilder.Create()
                    .WithIdentity("CircleScrapperTrigger", "Scrapper")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(24)
                        .RepeatForever()
                    ).Build();

                scheduler.ScheduleJob(circleScrapperJob, circleScrapperTrigger).Wait();
            }

            IJobDetail scheduleScrapperJob = JobBuilder.Create<ScheduleScrapperJob>().WithIdentity(ScheduleScrapperJob.Key).Build();

            if (!scheduler.CheckExists(scheduleScrapperJob.Key).Result)
            {
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
}
