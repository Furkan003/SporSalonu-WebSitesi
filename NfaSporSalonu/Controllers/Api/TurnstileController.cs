using Microsoft.AspNetCore.Mvc;
using NfaSporSalonu.Models.DTOs;
using NfaSporSalonu.Services;

namespace NfaSporSalonu.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class TurnstileController : ControllerBase
{
    private readonly ITurnstileService _turnstileService;

    public TurnstileController(ITurnstileService turnstileService)
    {
        _turnstileService = turnstileService;
    }

    /// <summary>
    /// QR kod ile turnike doğrulaması.
    /// POST /api/turnstile/verify
    /// Donanım cihazları için JWT gerektirmez.
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] TurnstileVerifyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.QrCode))
            return BadRequest(new ApiErrorResponse { StatusCode = 400, Message = "QR kod boş olamaz." });

        var result = await _turnstileService.VerifyAccess(request.QrCode);

        if (result.IsGranted)
            return Ok(result);

        // Geçersiz QR kod — kayıt bulunamadı
        if (result.MemberName == null)
            return NotFound(result);

        // Süresi dolmuş veya hesap devre dışı
        return StatusCode(StatusCodes.Status403Forbidden, result);
    }
}
