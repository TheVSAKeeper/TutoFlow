namespace TutoFlow.ApiService.Data.Enums;

/// <summary>
/// Способ присоединения репетитора к центру.
/// </summary>
internal enum JoinMethod
{
    /// <summary>Не определён.</summary>
    None = 0,

    /// <summary>По приглашению.</summary>
    Invitation = 1,

    /// <summary>Самостоятельная заявка.</summary>
    SelfRequest = 2,

    /// <summary>Добавлен администратором.</summary>
    AdminAdded = 3,

    /// <summary>По ссылке.</summary>
    ByLink = 4,
}
