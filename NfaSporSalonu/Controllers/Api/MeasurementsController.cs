using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MeasurementsController : ControllerBase
{
    private readonly NfaSporSalonuDbContext _context;

    public MeasurementsController(NfaSporSalonuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Üyenin tüm ölçümlerini listeler.
    /// GET /api/measurements
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var measurements = await _context.MemberMeasurements
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.MeasurementDate)
            .Select(m => new MeasurementResponseDto
            {
                MeasurementId = m.MeasurementId,
                MeasurementDate = m.MeasurementDate,
                Height = m.Height,
                Weight = m.Weight,
                Bicep = m.Bicep,
                Chest = m.Chest,
                Waist = m.Waist,
                Notes = m.Notes
            })
            .ToListAsync();

        return Ok(measurements);
    }

    /// <summary>
    /// Tekil ölçüm detayı.
    /// GET /api/measurements/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var measurement = await _context.MemberMeasurements
            .Where(m => m.MeasurementId == id && m.UserId == userId)
            .Select(m => new MeasurementResponseDto
            {
                MeasurementId = m.MeasurementId,
                MeasurementDate = m.MeasurementDate,
                Height = m.Height,
                Weight = m.Weight,
                Bicep = m.Bicep,
                Chest = m.Chest,
                Waist = m.Waist,
                Notes = m.Notes
            })
            .FirstOrDefaultAsync();

        if (measurement == null)
            return NotFound(new ApiErrorResponse { StatusCode = 404, Message = "Ölçüm bulunamadı." });

        return Ok(measurement);
    }

    /// <summary>
    /// Yeni ölçüm ekle.
    /// POST /api/measurements
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeasurementRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var measurement = new MemberMeasurement
        {
            UserId = userId,
            Height = request.Height,
            Weight = request.Weight,
            Bicep = request.Bicep,
            Chest = request.Chest,
            Waist = request.Waist,
            Notes = request.Notes,
            MeasurementDate = DateTime.Now
        };

        _context.MemberMeasurements.Add(measurement);
        await _context.SaveChangesAsync();

        var response = new MeasurementResponseDto
        {
            MeasurementId = measurement.MeasurementId,
            MeasurementDate = measurement.MeasurementDate,
            Height = measurement.Height,
            Weight = measurement.Weight,
            Bicep = measurement.Bicep,
            Chest = measurement.Chest,
            Waist = measurement.Waist,
            Notes = measurement.Notes
        };

        return CreatedAtAction(nameof(GetById), new { id = measurement.MeasurementId }, response);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
