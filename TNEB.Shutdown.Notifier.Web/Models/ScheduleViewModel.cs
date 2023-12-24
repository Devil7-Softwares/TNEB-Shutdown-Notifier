using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Models
{
    public class ScheduleViewModel(Circle circle, int pageNumber, int pageSize, string query, int totalCount, Schedule[] scheduleEntries)
    {
        public Circle Circle { get; } = circle;
        public int PageNumber { get; } = pageNumber;
        public int PageSize { get; } = pageSize;
        public string Query { get; } = query;
        public int TotalCount { get; } = totalCount;
        public int MaxPageNumber { get; } = (int)Math.Ceiling((double)totalCount / pageSize);
        public Schedule[] Schedules { get; } = scheduleEntries;
    }
}
