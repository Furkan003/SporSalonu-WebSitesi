using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WorkoutsController : ControllerBase
{
    private readonly NfaSporSalonu.Models.NfaSporSalonuDbContext _context;

    public WorkoutsController(NfaSporSalonu.Models.NfaSporSalonuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Üyeye atanmış tüm programları listeler.
    /// GET /api/workouts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var programs = await _context.WorkoutAndDietPrograms
            .Include(p => p.Trainer)
            .Where(p => p.TraineeId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new WorkoutResponseDto
            {
                ProgramId = p.ProgramId,
                Title = p.Title,
                ProgramType = p.ProgramType,
                Content = p.Content,
                TrainerName = p.Trainer != null
                    ? p.Trainer.FirstName + " " + p.Trainer.LastName
                    : null,
                CreatedDate = p.CreatedDate,
                EndDate = p.EndDate
            })
            .ToListAsync();

        return Ok(programs);
    }

    /// <summary>
    /// Program detayı.
    /// GET /api/workouts/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var program = await _context.WorkoutAndDietPrograms
            .Include(p => p.Trainer)
            .Where(p => p.ProgramId == id && p.TraineeId == userId)
            .Select(p => new WorkoutResponseDto
            {
                ProgramId = p.ProgramId,
                Title = p.Title,
                ProgramType = p.ProgramType,
                Content = p.Content,
                TrainerName = p.Trainer != null
                    ? p.Trainer.FirstName + " " + p.Trainer.LastName
                    : null,
                CreatedDate = p.CreatedDate,
                EndDate = p.EndDate
            })
            .FirstOrDefaultAsync();

        if (program == null)
            return NotFound(new ApiErrorResponse { StatusCode = 404, Message = "Program bulunamadı." });

        return Ok(program);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
