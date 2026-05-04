namespace NfaSporSalonu.Models.DTOs;

public class TurnstileVerifyRequest
{
    public string QrCode { get; set; } = null!;
}

public class TurnstileVerifyResponse
{
    public bool IsGranted { get; set; }
    public string Message { get; set; } = null!;
    public string? MemberName { get; set; }
    public string? PlanName { get; set; }
    public DateTime? MembershipEndDate { get; set; }
    public DateTime AccessTime { get; set; }
}
