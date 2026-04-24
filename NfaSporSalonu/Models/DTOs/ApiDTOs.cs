namespace NfaSporSalonu.Models.DTOs;

// ═══════════════ AUTH ═══════════════

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = null!;
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Role { get; set; }
    public string? ProfileImageUrl { get; set; }
}

// ═══════════════ MEASUREMENT ═══════════════

public class MeasurementResponseDto
{
    public int MeasurementId { get; set; }
    public DateTime? MeasurementDate { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Bicep { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public string? Notes { get; set; }
}

public class CreateMeasurementRequest
{
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Bicep { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public string? Notes { get; set; }
}

// ═══════════════ WORKOUT ═══════════════

public class WorkoutResponseDto
{
    public int ProgramId { get; set; }
    public string Title { get; set; } = null!;
    public string ProgramType { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? TrainerName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive => EndDate == null || EndDate > DateTime.Now;
}

// ═══════════════ COMMON ═══════════════

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
