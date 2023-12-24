using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNEB.Shutdown.Scrapper;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    public class ScrappedSchedule : Scrapper.ScrappedSchedule
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid? CircleId { get; set; }

        [ForeignKey(nameof(CircleId))]
        public Circle? Circle { get; set; }

        public ScrappedSchedule() : base()
        {
            Id = Guid.NewGuid();
            CircleId = Guid.Empty;
        }

        public ScrappedSchedule(Circle circle, ISchedule schedule) : base()
        {

            Id = Guid.NewGuid();
            Date = schedule.Date;
            From = schedule.From;
            To = schedule.To;
            Town = schedule.Town;
            SubStation = schedule.SubStation;
            Feeder = schedule.Feeder;
            TypeOfWork = schedule.TypeOfWork;
            CircleId = circle.Id;
            Circle = circle;
            Location = schedule.Location;
        }
    }
}
