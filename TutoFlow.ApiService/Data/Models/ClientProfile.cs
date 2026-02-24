#pragma warning disable MA0048, MA0051
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Профиль клиента (родитель или взрослый ученик).
/// </summary>
internal sealed class ClientProfile
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя.</summary>
    public int UserId { get; set; }

    /// <summary>Полное имя.</summary>
    public string FullName { get; set; } = null!;

    /// <summary>Дата рождения.</summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>Является ли клиент совершеннолетним.</summary>
    public bool IsAdult { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Связанный пользователь.</summary>
    public User User { get; set; } = null!;

    /// <summary>Ученики, привязанные к клиенту.</summary>
    public ICollection<Student> Students { get; } = [];
}

/// <summary>
/// Конфигурация сущности <see cref="ClientProfile" /> для EF Core.
/// </summary>
internal sealed class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
    public void Configure(EntityTypeBuilder<ClientProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("client_profiles");

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

        builder.Property(e => e.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(e => e.IsAdult)
            .HasColumnName("is_adult")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.User)
            .WithOne(u => u.ClientProfile)
            .HasForeignKey<ClientProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId)
            .IsUnique();
    }
}
