using System;

namespace NfaSporSalonu.Models;

/// <summary>
/// Admin tarafından yapılan manuel üyelik atamalarını ve
/// diğer yönetimsel işlemleri loglamak için kullanılır.
/// </summary>
public partial class AdminActivityLog
{
    public int LogId { get; set; }

    /// <summary>
    /// İşlemi yapan admin kullanıcısının Id'si.
    /// </summary>
    public int AdminUserId { get; set; }

    /// <summary>
    /// İşlemin yapıldığı hedef üye kullanıcısının Id'si.
    /// </summary>
    public int TargetUserId { get; set; }

    /// <summary>
    /// Yapılan işlemin türü (örn: "SubscriptionAssigned", "RoleChanged", "SubscriptionExtended").
    /// </summary>
    public string ActionType { get; set; } = null!;

    /// <summary>
    /// İşlem hakkında detaylı açıklama.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tanımlanan üyelik süresi (gün cinsinden). Üyelik atamalarında kullanılır.
    /// </summary>
    public int? DurationInDays { get; set; }

    /// <summary>
    /// İşlemin gerçekleştirildiği tarih ve saat.
    /// </summary>
    public DateTime ActionDate { get; set; }

    // Navigation Properties
    public virtual User AdminUser { get; set; } = null!;
    public virtual User TargetUser { get; set; } = null!;
}
