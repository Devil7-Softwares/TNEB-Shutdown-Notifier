using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<LocationStandardization> LocationStandardization { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<Schedule> scheduleEntryBuilder = modelBuilder
                 .Entity<Schedule>();

            scheduleEntryBuilder.Property(e => e.Date).HasConversion(Converters.DateTimeOffsetToUnix);
            scheduleEntryBuilder.Property(e => e.From).HasConversion(Converters.DateTimeOffsetToUnix);
            scheduleEntryBuilder.Property(e => e.To).HasConversion(Converters.DateTimeOffsetToUnix);
        }

        /// <summary>
        /// Save changes to the database without affecting other entities
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of entity to save changes for
        /// </typeparam>
        /// Source: https://stackoverflow.com/a/33404229
        public async Task<int> SaveChangesAsync<TEntity>() where TEntity : class
        {
            var original = this.ChangeTracker.Entries()
                        .Where(x => !typeof(TEntity).IsAssignableFrom(x.Entity.GetType()) && x.State != EntityState.Unchanged)
                        .GroupBy(x => x.State)
                        .ToList();

            foreach (var entry in this.ChangeTracker.Entries().Where(x => !typeof(TEntity).IsAssignableFrom(x.Entity.GetType())))
            {
                entry.State = EntityState.Unchanged;
            }

            var rows = await base.SaveChangesAsync();

            foreach (var state in original)
            {
                foreach (var entry in state)
                {
                    entry.State = state.Key;
                }
            }

            return rows;
        }

        public int SaveChanges<TEntity>() where TEntity : class
        {
            return SaveChangesAsync<TEntity>().GetAwaiter().GetResult();
        }

    }
}
