using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    [Index(nameof(CircleId), nameof(Name), IsUnique = true)]
    public class Location(Guid circleId, string name)
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = name;

        public Guid? CircleId { get; set; } = circleId;

        [ForeignKey(nameof(CircleId))]
        public Circle? Circle { get; set; }

        public Location() : this(Guid.NewGuid())
        {
        }

        public Location(Circle circle) : this(circle.Id)
        {
        }

        public Location(Guid circleId) : this(circleId, string.Empty)
        {
        }

        public Location(Circle circle, string name) : this(circle.Id, name)
        {
        }
    }
}
