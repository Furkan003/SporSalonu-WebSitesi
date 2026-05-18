using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace NfaSporSalonu.Services;

/// <summary>
/// QRCoder kullanarak üye giriş QR kodlarını üreten servis.
/// QR payload'ı: HMACSHA256 ile imzalanmış, manipülasyona karşı korumalı JSON formatında.
/// </summary>
public class QrCodeService : IQrCodeService
{
    private readonly byte[] _secretKey;

    public QrCodeService(IConfiguration configuration)
    {
        // appsettings.json → QrSettings:SecretKey (yoksa fallback)
        var keyString = configuration["QrSettings:SecretKey"] ?? "NfaSporSalonu-QR-DefaultKey-2026!";
        _secretKey = Encoding.UTF8.GetBytes(keyString);
    }

    public string GenerateQrCodeBase64(int userId, string fullName)
    {
        // ── 1. Payload oluştur (imzalı) ──
        var payload = BuildSignedPayload(userId, fullName);

        // ── 2. QR Kod üret ──
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        byte[] qrBytes = qrCode.GetGraphic(8,
            new byte[] { 30, 30, 30 },       // koyu ön plan
            new byte[] { 255, 255, 255 });    // beyaz arka plan

        var base64 = Convert.ToBase64String(qrBytes);
        return $"data:image/png;base64,{base64}";
    }

    /// <summary>
    /// UserId + FullName + timestamp içeren JSON payload'ı HMACSHA256 ile imzalar.
    /// Turnikede doğrulama yapılırken aynı anahtar ile imza kontrol edilir.
    /// </summary>
    private string BuildSignedPayload(int userId, string fullName)
    {
        var data = $"{{\"uid\":{userId},\"name\":\"{EscapeJson(fullName)}\",\"iat\":\"{DateTime.UtcNow:O}\"}}";

        using var hmac = new HMACSHA256(_secretKey);
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var signature = Convert.ToBase64String(signatureBytes);

        return $"{{\"data\":{data},\"sig\":\"{signature}\"}}";
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}
