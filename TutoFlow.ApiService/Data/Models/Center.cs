using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Репетиторский центр.
/// </summary>
public class Center
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Название центра.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Юридическое наименование.</summary>
    public string? LegalName { get; set; }

    /// <summary>ИНН (10–12 цифр).</summary>
    public string? Inn { get; set; }

    /// <summary>Адрес.</summary>
    public string? Address { get; set; }

    /// <summary>Номер телефона.</summary>
    public string? Phone { get; set; }

    /// <summary>Адрес электронной почты.</summary>
    public string? Email { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Верифицирован ли центр.</summary>
    public bool IsVerified { get; set; }

    /// <summary>Профили администраторов центра.</summary>
    public ICollection<AdminProfile> AdminProfiles { get; set; } = [];

    /// <summary>Записи о членстве репетиторов.</summary>
    public ICollection<CenterMembership> Memberships { get; set; } = [];
}

/// <summary>
/// Конфигурация сущности <see cref="Center" /> для EF Core.
/// </summary>
public class CenterConfiguration : IEntityTypeConfiguration<Center>
{
    public void Configure(EntityTypeBuilder<Center> builder)
    {
        builder.ToTable("centers", t =>
        {
            t.HasCheckConstraint("chk_centers_inn", "inn ~ '^\\d{10,12}$'");
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(255);

        builder.Property(e => e.Inn)
            .HasColumnName("inn")
            .HasMaxLength(12);

        builder.Property(e => e.Address)
            .HasColumnName("address");

        builder.Property(e => e.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValue(false);

        builder.HasIndex(e => e.Inn)
            .IsUnique()
            .HasDatabaseName("idx_centers_inn")
            .HasFilter("inn IS NOT NULL");

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("idx_centers_name");
    }
}
