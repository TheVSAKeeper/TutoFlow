#pragma warning disable MA0048, MA0051
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Ученик, привязанный к клиенту.
/// </summary>
internal sealed class Student
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор профиля клиента.</summary>
    public int ClientProfileId { get; set; }

    /// <summary>Полное имя ученика.</summary>
    public string FullName { get; set; } = null!;

    /// <summary>Класс обучения (1–12).</summary>
    public short? Grade { get; set; }

    /// <summary>Дата рождения.</summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>Является ли ученик самим клиентом.</summary>
    public bool IsSelf { get; set; }

    /// <summary>Примечания.</summary>
    public string? Notes { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Профиль клиента-родителя.</summary>
    public ClientProfile ClientProfile { get; set; } = null!;
}

/// <summary>
/// Конфигурация сущности <see cref="Student" /> для EF Core.
/// </summary>
internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("students", t =>
        {
            t.HasCheckConstraint("chk_students_grade", "grade BETWEEN 1 AND 12");
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.ClientProfileId)
            .HasColumnName("client_profile_id")
            .IsRequired();

        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Grade)
            .HasColumnName("grade");

        builder.Property(e => e.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(e => e.IsSelf)
            .HasColumnName("is_self")
            .HasDefaultValue(false);

        builder.Property(e => e.Notes)
            .HasColumnName("notes");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.ClientProfile)
            .WithMany(c => c.Students)
            .HasForeignKey(e => e.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ClientProfileId)
            .HasDatabaseName("idx_students_client_profile_id");

        builder.HasIndex(e => e.FullName)
            .HasDatabaseName("idx_students_full_name");
    }
}
