using System;

namespace NfaSporSalonu.Models;

/// <summary>
/// Turnikeden QR kod ile geçiş yapan kullanıcıların logları.
/// Her QR okutma bir kayıt oluşturur.
/// </summary>
public partial class QRAccessLog
{
    public int Id { get; set; }

    /// <summary>
    /// QR ile geçiş yapan kullanıcının Id'si.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Geçiş zamanı.
    /// </summary>
    public DateTime AccessTime { get; set; }

    /// <summary>
    /// Geçiş durumu (örn: "Granted", "Denied", "Expired").
    /// </summary>
    public string Status { get; set; } = null!;

    // Navigation Property
    public virtual User? User { get; set; }
}
