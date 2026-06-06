using System;

namespace NfaSporSalonu.Models;

public partial class SupportTicket
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Category { get; set; } = null!; // İstek, Dilek, Şikayet
    public string Subject { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Status { get; set; } = "Bekliyor"; // Bekliyor, İnceleniyor, Yanıtlandı, Kapatıldı
    public string? AdminResponse { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public virtual User? User { get; set; }
}
