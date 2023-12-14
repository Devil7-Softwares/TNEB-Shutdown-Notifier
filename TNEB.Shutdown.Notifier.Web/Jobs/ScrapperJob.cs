using Quartz;

namespace TNEB.Shutdown.Notifier.Web.Jobs
{
    public class ScrapperJob : IJob
    {
        private readonly ILogger<ScrapperJob> logger;

        public ScrapperJob(ILogger<ScrapperJob> logger)
        {
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("ScrapperJob started");
            return Task.CompletedTask;
        }
    }

    internal static class ScrapperJobExtensions
    {
        public static void ScheduleScrapper(this WebApplication app)
        {
            ISchedulerFactory schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;

            IJobDetail job = JobBuilder.Create<ScrapperJob>().WithIdentity("ScrapperJob", "Scrapper").Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ScrapperTrigger", "Scrapper")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(1)
                    .RepeatForever()
                ).Build();

            scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}
