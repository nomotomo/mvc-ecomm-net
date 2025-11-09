using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ordering.Core.Common;
using Ordering.Core.Entities;

namespace Ordering.Infrastructure.Data;
public class OrderContext(DbContextOptions<OrderContext> options, IHttpContextAccessor httpContextAccessor)
    : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OutBoxMessage> OutBoxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<OutBoxMessage>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.HasIndex(o => o.CorrelationId).IsUnique();
            builder.Property(o => o.Type).IsRequired();
            builder.Property(o => o.Content).IsRequired();
            builder.Property(o => o.OccuredOn).IsRequired();
            builder.Property(o => o.ProcessedOn);
        });
        modelBuilder.Entity<Order>().Property(o => o.Status).HasConversion<string>();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        // Try to get the current username from the HTTP context
        var username = httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "system";
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = DateTime.UtcNow;
                    entry.Entity.CreatedBy = username;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedOn = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = username;
                    break;
            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}