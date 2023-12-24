using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TNEB.Shutdown.Notifier.Web.Data;
using TNEB.Shutdown.Notifier.Web.Data.Models;
using TNEB.Shutdown.Notifier.Web.Models;

namespace TNEB.Shutdown.Notifier.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            Circle[] circles = _dbContext.Circles.OrderBy(c => c.Name).ToArray();

            return View(new HomeViewModel(circles));
        }

        [HttpPost]
        public IActionResult Index(Guid circleId)
        {
            return RedirectToAction("Schedule", new { circleId });
        }

        public IActionResult Schedule(Guid circleId, int pageNumber = 1, int pageSize = 20, string q = "")
        {
            Circle? circle = _dbContext.Circles.Find(circleId);

            if (circle == null)
            {
                return RedirectToAction("Index");
            }

            Schedule[] scheduleEntries = _dbContext.Schedules
                .Include(s => s.Location)
                .Where(s =>
                    string.IsNullOrEmpty(q) ?
                    s.CircleId == circleId :
                    s.CircleId == circleId && (
                        s.Town.Contains(q) ||
                        s.SubStation.Contains(q) ||
                        s.Feeder.Contains(q) ||
                        s.Location!.Name.Contains(q)
                    )
                )
                .OrderByDescending(s => s.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            int totalCount = _dbContext.Schedules.Count(s => s.CircleId == circleId);

            return View(new ScheduleViewModel(circle, pageNumber, pageSize, q, totalCount, scheduleEntries));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
