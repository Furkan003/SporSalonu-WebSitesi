using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Services;

public interface ITurnstileService
{
    Task<TurnstileVerifyResponse> VerifyAccess(string qrCode);
}
