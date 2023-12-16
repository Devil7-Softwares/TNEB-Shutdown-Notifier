using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Models
{
    public class ScheduleViewModel
    {
        public CircleEntry Circle { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public string Query { get; }
        public int TotalCount { get; }
        public int MaxPageNumber { get; }
        public ScheduleEntry[] Schedules { get; }

        public ScheduleViewModel(CircleEntry circle, int pageNumber, int pageSize, string query, int totalCount, ScheduleEntry[] scheduleEntries)
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
