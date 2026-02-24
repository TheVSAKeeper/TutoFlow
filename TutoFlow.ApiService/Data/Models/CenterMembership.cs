#pragma warning disable MA0048, MA0051
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoFlow.ApiService.Data.Enums;

namespace TutoFlow.ApiService.Data.Models;

/// <summary>
/// Членство репетитора в центре (история участия).
/// </summary>
internal sealed class CenterMembership
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор центра.</summary>
    public int CenterId { get; set; }

    /// <summary>Идентификатор профиля репетитора.</summary>
    public int TutorProfileId { get; set; }

    /// <summary>Идентификатор администратора, одобрившего вступление.</summary>
    public int? ApprovedByAdminId { get; set; }

    /// <summary>Дата и время вступления.</summary>
    public DateTimeOffset JoinedAt { get; set; }

    /// <summary>Дата и время выхода из центра.</summary>
    public DateTimeOffset? LeftAt { get; set; }

    /// <summary>Способ присоединения.</summary>
    public JoinMethod JoinMethod { get; set; }

    /// <summary>Текущий статус членства.</summary>
    public MembershipStatus Status { get; set; }

    /// <summary>Причина выхода из центра.</summary>
    public string? LeaveReason { get; set; }

    /// <summary>Примечания.</summary>
    public string? Notes { get; set; }

    /// <summary>Дата и время создания записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Дата и время последнего обновления.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Центр.</summary>
    public Center Center { get; set; } = null!;

    /// <summary>Профиль репетитора.</summary>
    public TutorProfile TutorProfile { get; set; } = null!;

    /// <summary>Администратор, одобривший вступление.</summary>
    public AdminProfile? ApprovedByAdmin { get; set; }
}

/// <summary>
/// Конфигурация сущности <see cref="CenterMembership" /> для EF Core.
/// </summary>
internal sealed class CenterMembershipConfiguration : IEntityTypeConfiguration<CenterMembership>
{
    public void Configure(EntityTypeBuilder<CenterMembership> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("center_membership");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.CenterId)
            .HasColumnName("center_id")
            .IsRequired();

        builder.Property(e => e.TutorProfileId)
            .HasColumnName("tutor_profile_id")
            .IsRequired();

        builder.Property(e => e.ApprovedByAdminId)
            .HasColumnName("approved_by_admin_id");

        builder.Property(e => e.JoinedAt)
            .HasColumnName("joined_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.LeftAt)
            .HasColumnName("left_at");

        builder.Property(e => e.JoinMethod)
            .HasColumnName("join_method")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasDefaultValue(MembershipStatus.Active)
            .HasSentinel(MembershipStatus.None);

        builder.Property(e => e.LeaveReason)
            .HasColumnName("leave_reason");

        builder.Property(e => e.Notes)
            .HasColumnName("notes");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.Center)
            .WithMany(c => c.Memberships)
            .HasForeignKey(e => e.CenterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TutorProfile)
            .WithMany(t => t.Memberships)
            .HasForeignKey(e => e.TutorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ApprovedByAdmin)
            .WithMany(a => a.ApprovedMemberships)
            .HasForeignKey(e => e.ApprovedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.CenterId)
            .HasDatabaseName("idx_center_membership_center_id");

        builder.HasIndex(e => e.TutorProfileId)
            .HasDatabaseName("idx_center_membership_tutor_profile_id");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_center_membership_status");

        builder.HasIndex(e => e.JoinedAt)
            .HasDatabaseName("idx_center_membership_joined_at");

        builder.HasIndex(e => new { e.CenterId, e.TutorProfileId })
            .IsUnique()
            .HasDatabaseName("idx_unique_active_membership")
            .HasFilter("status = 'active'");
    }
}
