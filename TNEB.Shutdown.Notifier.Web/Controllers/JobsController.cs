using Microsoft.AspNetCore.Mvc;
using Quartz;
using TNEB.Shutdown.Notifier.Web.Jobs;
using TNEB.Shutdown.Notifier.Web.Utils;

namespace TNEB.Shutdown.Notifier.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [BasicAuthorize]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> _logger;
        private readonly ISchedulerFactory _schedulerFactory;

        public JobsController(ILogger<JobsController> logger, ISchedulerFactory schedulerFactory)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
        }

        /// <summary>
        /// Manually trigger the circle scrapper job.
        /// </summary>
        [HttpGet("scrapper/circle/trigger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TriggerCircleScrapperJob()
        {
            _logger.LogInformation("Triggering circle scrapper job manually.");
            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(CircleScrapperJob.Key);
            return Ok();
        }

        /// <summary>
        /// Manually trigger the schedule scrapper job.
        /// </summary>
        [HttpGet("scrapper/schedule/trigger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TriggerScheduleScrapperJob()
        {
            _logger.LogInformation("Triggering schedule scrapper job manually.");
            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(ScheduleScrapperJob.Key);
            return Ok();
        }
    }
}
