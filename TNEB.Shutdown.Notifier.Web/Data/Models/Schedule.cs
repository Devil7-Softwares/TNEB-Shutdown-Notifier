﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    public class Schedule
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
        public Circle? Circle { get; set; }

        [Required]
        public Guid ScrappedScheduleId { get; set; }

        [ForeignKey(nameof(ScrappedScheduleId))]
        public ScrappedSchedule? ScrappedSchedule { get; set; }

        public Schedule()
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
            ScrappedScheduleId = Guid.Empty;
            ScrappedSchedule = null;
        }

        public Schedule(Circle circle, Location location, ScrappedSchedule schedule)
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
            ScrappedScheduleId = schedule.Id;
            ScrappedSchedule = schedule;
        }
    }
}
