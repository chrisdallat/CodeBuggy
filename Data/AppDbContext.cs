using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace CodeBuggy.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<BurndownData> BurndownData { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Comment> Comments { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new AppUserEntityConfiguration());

        builder.Entity<BurndownData>()
            .OwnsMany(b => b.DailyCounts, dc =>
            {
                dc.Property(c => c.Date);
                dc.Property(c => c.ToDoCount);
                dc.Property(c => c.InProgressCount);
                dc.Property(c => c.ReviewCount);
                dc.Property(c => c.DoneCount);
                dc.Property(c => c.NonePriorityCount);
                dc.Property(c => c.LowPriorityCount);
                dc.Property(c => c.MediumPriorityCount);
                dc.Property(c => c.HighPriorityCount);
                dc.Property(c => c.UrgentPriorityCount);
            });
    }

}

public class AppUserEntityConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(255);
        builder.Property(u => u.LastName).HasMaxLength(255);
    }
}