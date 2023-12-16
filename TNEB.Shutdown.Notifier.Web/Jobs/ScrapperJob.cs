using Quartz;
using TNEB.Shutdown.Notifier.Web.Data;
using TNEB.Shutdown.Notifier.Web.Data.Models;
using TNEB.Shutdown.Scrapper;

namespace TNEB.Shutdown.Notifier.Web.Jobs
{
    public class ScheduleScrapperJob : IJob
    {
        private readonly ILogger<ScheduleScrapperJob> logger;
        private readonly AppDbContext dbContext;

        private Location[]? locations;

        public ScheduleScrapperJob(ILogger<ScheduleScrapperJob> logger, AppDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        private async Task<Location> GetLocation(string locationName)
        {
            if (locations == null)
            {
                logger.LogDebug("Fetching locations...");
                locations = dbContext.Locations.ToArray();
                logger.LogInformation($"{locations.Length} Locations fetched!");
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
                    logger.LogDebug($"Adding location {locationName}...");

                    location = new Location
                    {
                        Id = Guid.NewGuid(),
                        Name = locationStandardization?.StandardizedLocation ?? locationName
                    };

                    dbContext.Locations.Add(location);

                    await dbContext.SaveChangesAsync<Location>();
                    logger.LogDebug($"Location {locationName} added!");
                }
            }

            return location;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            CircleEntry[] circleEntries = Array.Empty<CircleEntry>();

            try
            {
                logger.LogDebug("Fetching circles for fetching schedules...");
                circleEntries = dbContext.Circles.ToArray();
                logger.LogInformation($"{circleEntries.Length} Circles fetched for fetching schedules!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch circles for fetching schedules!");
                return;
            }

            if (circleEntries.Length == 0)
            {
                logger.LogWarning("No circles found for fetching schedules!");
                return;
            }

            Dictionary<CircleEntry, ISchedule[]> circleSchedule = new Dictionary<CircleEntry, ISchedule[]>();

            for (int i = 0; i < circleEntries.Length; i++)
            {
                CircleEntry circleEntry = circleEntries[i];
                logger.LogDebug($"[{i + 1}/{circleEntries.Length}] Fetching schedules for circle {circleEntry.Name} ({circleEntry.Value})...");

                try
                {
                    ISchedule[] circleSchedules = await Utils.GetSchedules(circleEntry.Value);
                    logger.LogInformation($"[{i + 1}/{circleEntries.Length}] {circleSchedules.Length} Schedules fetched for circle {circleEntry.Name} ({circleEntry.Value})!");

                    circleSchedule.Add(circleEntry, circleSchedules);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"[{i + 1}/{circleEntries.Length}] Failed to fetch schedules for circle {circleEntry.Name} ({circleEntry.Value})!");
                    continue;
                }
            }

            foreach (KeyValuePair<CircleEntry, ISchedule[]> keyValue in circleSchedule)
            {
                CircleEntry circle = keyValue.Key;
                ISchedule[] schedules = keyValue.Value;

                foreach (ISchedule schedule in schedules)
                {
                    string[] locationNames = schedule.Location.Split(',').Select(l => l.Trim()).TakeWhile(l => !string.IsNullOrWhiteSpace(l)).ToArray();

                    foreach (string locationName in locationNames)
                    {
                        Location location = await GetLocation(locationName);

                        ScheduleEntry? existingScheduleEntry = dbContext.Schedules.FirstOrDefault(s => s.Date == schedule.Date && s.From == schedule.From && s.To == schedule.To && s.LocationId == location.Id && s.CircleId == circle.Id);

                        if (existingScheduleEntry == null)
                        {
                            logger.LogDebug($"Adding schedule for {location.Name} ({schedule.Date.ToString("yyyy-MM-dd")})...");

                            ScheduleEntry scheduleEntry = new ScheduleEntry
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

                            logger.LogInformation($"Schedule for {location.Name} ({location.Id}) added!");
                        }
                        else
                        {
                            logger.LogDebug($"Schedule for {location.Name} ({location.Id}) already exists!");
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
