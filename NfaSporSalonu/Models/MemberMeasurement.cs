using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class MemberMeasurement
{
    public int MeasurementId { get; set; }

    public int? UserId { get; set; }

    public DateTime? MeasurementDate { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Bicep { get; set; }

    public decimal? Chest { get; set; }

    public decimal? Waist { get; set; }

    public string? Notes { get; set; }

    public virtual User? User { get; set; }
}
