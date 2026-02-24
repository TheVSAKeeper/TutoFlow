#pragma warning disable MA0048, MA0051
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoFlow.ApiService.Data.Enums;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Профиль администратора центра.
/// </summary>
internal sealed class AdminProfile
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя.</summary>
    public int UserId { get; set; }

    /// <summary>Идентификатор центра.</summary>
    public int CenterId { get; set; }

    /// <summary>Полное имя.</summary>
    public string FullName { get; set; } = null!;

    /// <summary>Должность.</summary>
    public string? JobTitle { get; set; }

    /// <summary>Уровень прав доступа.</summary>
    public PermissionsLevel PermissionsLevel { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Связанный пользователь.</summary>
    public User User { get; set; } = null!;

    /// <summary>Центр, к которому привязан администратор.</summary>
    public Center Center { get; set; } = null!;

    /// <summary>Записи о членстве, одобренные данным администратором.</summary>
    public ICollection<CenterMembership> ApprovedMemberships { get; } = [];
}

/// <summary>
/// Конфигурация сущности <see cref="AdminProfile" /> для EF Core.
/// </summary>
internal sealed class AdminProfileConfiguration : IEntityTypeConfiguration<AdminProfile>
{
    public void Configure(EntityTypeBuilder<AdminProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("admin_profiles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.CenterId)
            .HasColumnName("center_id")
            .IsRequired();

        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.JobTitle)
            .HasColumnName("job_title")
            .HasMaxLength(255);

        builder.Property(e => e.PermissionsLevel)
            .HasColumnName("permissions_level")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.User)
            .WithOne(u => u.AdminProfile)
            .HasForeignKey<AdminProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Center)
            .WithMany(c => c.AdminProfiles)
            .HasForeignKey(e => e.CenterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId)
            .IsUnique();

        builder.HasIndex(e => e.CenterId)
            .HasDatabaseName("idx_admin_profiles_center_id");
    }
}
