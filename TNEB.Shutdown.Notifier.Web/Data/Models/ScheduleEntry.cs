using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNEB.Shutdown.Scrapper;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    public class ScheduleEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset Date { get; set; }

        [Required]
        public DateTimeOffset From { get; set; }

        [Required]
        public DateTimeOffset To { get; set; }

        [Required]
        public string Town { get; set; }

        [Required]
        public string SubStation { get; set; }

        [Required]
        public string Feeder { get; set; }

        [Required]
        public string TypeOfWork { get; set; }

        [Required]
        public Guid LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        [Required]
        public Guid CircleId { get; set; }

        [ForeignKey(nameof(CircleId))]
        public CircleEntry? Circle { get; set; }

        public ScheduleEntry()
        {
            Id = Guid.NewGuid();
            Date = DateTimeOffset.MinValue;
            From = DateTimeOffset.MinValue;
            To = DateTimeOffset.MinValue;
            Town = string.Empty;
            SubStation = string.Empty;
            Feeder = string.Empty;
            TypeOfWork = string.Empty;
            LocationId = Guid.Empty;
            Location = null;
            CircleId = Guid.Empty;
            Circle = null;
        }

        public ScheduleEntry(CircleEntry circle, Location location, ISchedule schedule)
        {
            Id = Guid.NewGuid();
            Date = schedule.Date;
            From = schedule.From;
            To = schedule.To;
            Town = schedule.Town;
            SubStation = schedule.SubStation;
            Feeder = schedule.Feeder;
            TypeOfWork = schedule.TypeOfWork;
            LocationId = location.Id;
            Location = location;
            CircleId = circle.Id;
            Circle = circle;
        }
    }
}
