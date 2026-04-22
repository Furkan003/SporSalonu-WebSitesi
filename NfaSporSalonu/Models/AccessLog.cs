using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class AccessLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string AccessType { get; set; } = null!;

    public string Method { get; set; } = null!;

    public DateTime? AccessTime { get; set; }

    public bool IsGranted { get; set; }

    public string? DenialReason { get; set; }

    public virtual User? User { get; set; }
}
