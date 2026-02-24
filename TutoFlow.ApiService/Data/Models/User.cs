#pragma warning disable MA0048, MA0051
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoFlow.ApiService.Data.Enums;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Учётная запись пользователя.
/// </summary>
internal sealed class User
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Адрес электронной почты.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Хеш пароля.</summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>Номер телефона.</summary>
    public string? Phone { get; set; }

    /// <summary>Роль в системе.</summary>
    public UserRole Role { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Подтверждён ли адрес электронной почты.</summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>Дата и время подтверждения почты.</summary>
    public DateTimeOffset? ConfirmedAt { get; set; }

    /// <summary>Профиль репетитора (если роль — репетитор).</summary>
    public TutorProfile? TutorProfile { get; set; }

    /// <summary>Профиль клиента (если роль — клиент).</summary>
    public ClientProfile? ClientProfile { get; set; }

    /// <summary>Профиль администратора (если роль — администратор).</summary>
    public AdminProfile? AdminProfile { get; set; }
}

/// <summary>
/// Конфигурация сущности <see cref="User" /> для EF Core.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(e => e.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.IsEmailConfirmed)
            .HasColumnName("is_email_confirmed")
            .HasDefaultValue(false);

        builder.Property(e => e.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasIndex(e => e.Role)
            .HasDatabaseName("idx_users_role");
    }
}
