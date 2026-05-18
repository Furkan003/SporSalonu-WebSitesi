using System;

namespace NfaSporSalonu.Models;

/// <summary>
/// Üyelerin sisteme bıraktığı anket/yorumlar.
/// Rating 1-5 arası, admin onayı gerektirir.
/// </summary>
public partial class Feedback
{
    public int Id { get; set; }

    /// <summary>
    /// Yorumu bırakan kullanıcının Id'si.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 1 ile 5 arasında memnuniyet puanı.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Kullanıcının yazdığı yorum metni.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Admin tarafından onaylanıp onaylanmadığını belirtir.
    /// Varsayılan: false (onay bekliyor).
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Yorumun oluşturulma tarihi.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    // Navigation Property
    public virtual User? User { get; set; }
}
