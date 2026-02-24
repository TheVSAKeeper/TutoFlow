namespace TutoFlow.ApiService.Data.Enums;

/// <summary>
/// Уровень прав администратора центра.
/// </summary>
internal enum PermissionsLevel
{
    /// <summary>Не определён.</summary>
    None = 0,

    /// <summary>Суперадминистратор.</summary>
    SuperAdmin = 1,

    /// <summary>Администратор центра.</summary>
    CenterAdmin = 2,

    /// <summary>Модератор.</summary>
    Moderator = 3,
}
