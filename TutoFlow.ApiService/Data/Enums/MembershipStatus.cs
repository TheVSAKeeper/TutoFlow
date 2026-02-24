namespace TutoFlow.ApiService.Data.Enums;

/// <summary>
/// Статус членства репетитора в центре.
/// </summary>
internal enum MembershipStatus
{
    /// <summary>Не определён.</summary>
    None = 0,

    /// <summary>Активный участник.</summary>
    Active = 1,

    /// <summary>Покинул центр.</summary>
    Left = 2,

    /// <summary>Приостановлен.</summary>
    Suspended = 3,
}
