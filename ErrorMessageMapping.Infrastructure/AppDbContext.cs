using ErrorMessageMapping.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErrorMessageMapping.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ErrorMapping> ErrorMappings => Set<ErrorMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErrorMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MatchType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MatchValue1).IsRequired().HasMaxLength(300);
            entity.Property(e => e.MatchValue2).HasMaxLength(300);
            entity.Property(e => e.MatchValue3).HasMaxLength(300);
            entity.Property(e => e.ReplaceMessage).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
        });
    }
}