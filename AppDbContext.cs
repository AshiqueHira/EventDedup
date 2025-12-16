using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedEvent>()
            .HasIndex(e => e.EventId)
            .IsUnique();
    }
}