using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Models
{
    public class ScheduleViewModel
    {
        public Circle Circle { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public string Query { get; }
        public int TotalCount { get; }
        public int MaxPageNumber { get; }
        public Schedule[] Schedules { get; }

        public ScheduleViewModel(Circle circle, int pageNumber, int pageSize, string query, int totalCount, Schedule[] scheduleEntries)
        {
            Circle = circle;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Query = query;
            TotalCount = totalCount;
            Schedules = scheduleEntries;
            MaxPageNumber = (int)Math.Ceiling((double)totalCount / pageSize);
        }
    }
}
