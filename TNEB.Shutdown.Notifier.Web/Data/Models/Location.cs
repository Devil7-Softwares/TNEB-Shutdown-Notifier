using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Location()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
        }

        public Location(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}
