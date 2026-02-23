using Microsoft.EntityFrameworkCore;
using TutoFlow.ApiService.Data.Models;

namespace TutoFlow.ApiService.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Center> Centers => Set<Center>();

    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();

    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();

    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<CenterMembership> CenterMemberships => Set<CenterMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<Enums.UserRole>(name: "user_role");
        modelBuilder.HasPostgresEnum<Enums.WorkModel>(name: "work_model");
        modelBuilder.HasPostgresEnum<Enums.PermissionsLevel>(name: "permissions_level");
        modelBuilder.HasPostgresEnum<Enums.JoinMethod>(name: "join_method");
        modelBuilder.HasPostgresEnum<Enums.MembershipStatus>(name: "membership_status");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
