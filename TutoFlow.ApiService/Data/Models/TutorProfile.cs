#pragma warning disable MA0048, MA0051
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoFlow.ApiService.Data.Enums;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Профиль репетитора.
/// </summary>
internal sealed class TutorProfile
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя.</summary>
    public int UserId { get; set; }

    /// <summary>Полное имя.</summary>
    public string FullName { get; set; } = null!;

    /// <summary>Биография.</summary>
    public string? Biography { get; set; }

    /// <summary>Специализация (предметы).</summary>
    public string? Specialization { get; set; }

    /// <summary>Стоимость часа занятия.</summary>
    public decimal? HourlyRate { get; set; }

    /// <summary>Образование.</summary>
    public string? Education { get; set; }

    /// <summary>Опыт работы в годах.</summary>
    public short? ExperienceYears { get; set; }

    /// <summary>Модель работы (индивидуально / в центре).</summary>
    public WorkModel WorkModel { get; set; }

    /// <summary>Верифицирован ли профиль.</summary>
    public bool ProfileVerified { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Связанный пользователь.</summary>
    public User User { get; set; } = null!;

    /// <summary>Записи о членстве в центрах.</summary>
    public ICollection<CenterMembership> Memberships { get; } = [];
}

/// <summary>
/// Конфигурация сущности <see cref="TutorProfile" /> для EF Core.
/// </summary>
internal sealed class TutorProfileConfiguration : IEntityTypeConfiguration<TutorProfile>
{
    public void Configure(EntityTypeBuilder<TutorProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tutor_profiles", t =>
        {
            t.HasCheckConstraint("chk_tutor_hourly_rate", "hourly_rate > 0");
            t.HasCheckConstraint("chk_tutor_experience", "experience_years >= 0");
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Biography)
            .HasColumnName("biography");

        builder.Property(e => e.Specialization)
            .HasColumnName("specialization")
            .HasMaxLength(255);

        builder.Property(e => e.HourlyRate)
            .HasColumnName("hourly_rate")
            .HasPrecision(10, 2);

        builder.Property(e => e.Education)
            .HasColumnName("education");

        builder.Property(e => e.ExperienceYears)
            .HasColumnName("experience_years");

        builder.Property(e => e.WorkModel)
            .HasColumnName("work_model")
            .IsRequired();

        builder.Property(e => e.ProfileVerified)
            .HasColumnName("profile_verified")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.User)
            .WithOne(u => u.TutorProfile)
            .HasForeignKey<TutorProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId)
            .IsUnique();

        builder.HasIndex(e => e.Specialization)
            .HasDatabaseName("idx_tutor_profiles_specialization");

        builder.HasIndex(e => e.WorkModel)
            .HasDatabaseName("idx_tutor_profiles_work_model");
    }
}
