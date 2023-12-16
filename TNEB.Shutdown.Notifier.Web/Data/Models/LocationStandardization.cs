using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    public class LocationStandardization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string StandardizedLocation { get; set; }

        public LocationStandardization()
        {
            Id = Guid.NewGuid();
            Location = string.Empty;
            StandardizedLocation = string.Empty;
        }

        public LocationStandardization(string location, string standardizedLocation)
        {
            Id = Guid.NewGuid();
            Location = location;
            StandardizedLocation = standardizedLocation;
        }
    }
}
