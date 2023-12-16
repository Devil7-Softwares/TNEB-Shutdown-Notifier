using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<LocationStandardization> LocationStandardization { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<CircleEntry> Circles { get; set; }
        public DbSet<ScheduleEntry> Schedules { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<ScheduleEntry> scheduleEntryBuilder = modelBuilder
                 .Entity<ScheduleEntry>();

            scheduleEntryBuilder.Property(e => e.Date).HasConversion(Converters.DateTimeOffsetToUnix);
            scheduleEntryBuilder.Property(e => e.From).HasConversion(Converters.DateTimeOffsetToUnix);
            scheduleEntryBuilder.Property(e => e.To).HasConversion(Converters.DateTimeOffsetToUnix);
        }
    }
}
