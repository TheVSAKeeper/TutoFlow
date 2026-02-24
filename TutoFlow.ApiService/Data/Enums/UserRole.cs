namespace TutoFlow.ApiService.Data.Enums;

/// <summary>
/// Роль пользователя в системе.
/// </summary>
internal enum UserRole
{
    /// <summary>Не определена.</summary>
    None = 0,

    /// <summary>Клиент (родитель или взрослый ученик).</summary>
    Client = 1,

    /// <summary>Репетитор.</summary>
    Tutor = 2,

    /// <summary>Администратор центра.</summary>
    Admin = 3,

    /// <summary>Суперадминистратор системы.</summary>
    SuperAdmin = 4,
}
