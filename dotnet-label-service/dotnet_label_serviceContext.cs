using Microsoft.EntityFrameworkCore;
using dotnet_label_service.Models;

public class dotnet_label_serviceContext(DbContextOptions<dotnet_label_serviceContext> options) : DbContext(options)
{
    public DbSet<Track> Track { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Track>(entity =>
        {
            entity.Property(t => t.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("NOW()");
        });
    }
}
