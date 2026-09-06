using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Persistence;

public class TmsDbContext : IdentityDbContext<TmsUser>
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options)
    {
        
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This line tells EF Core to find and apply all configurations
        // that implement IEntityTypeConfiguration in the same assembly as TmsDbContext.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);

        // Call the base method
        base.OnModelCreating(modelBuilder);
    }


}

