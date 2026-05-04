using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NfaSporSalonu.Models.DTOs;
using NfaSporSalonu.Services;

namespace NfaSporSalonu.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Üyenin gelişim raporu — ilk vs son ölçüm karşılaştırması.
    /// GET /api/analytics/progress
    /// </summary>
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var report = await _analyticsService.GetProgressReport(userId.Value);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>
    /// Zaman serisi trend verileri — Chart.js uyumlu JSON.
    /// GET /api/analytics/trends
    /// </summary>
    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var trends = await _analyticsService.GetMeasurementTrends(userId.Value);
        return Ok(trends);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
