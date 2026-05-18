namespace NfaSporSalonu.Services;

/// <summary>
/// Üyelere ait giriş QR kodlarını üreten servis arayüzü.
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Verilen kullanıcı bilgisine göre Base64 formatında PNG QR kod üretir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="fullName">Kullanıcının tam adı (QR payload'a eklenir)</param>
    /// <returns>data:image/png;base64,... formatında string</returns>
    string GenerateQrCodeBase64(int userId, string fullName);
}
