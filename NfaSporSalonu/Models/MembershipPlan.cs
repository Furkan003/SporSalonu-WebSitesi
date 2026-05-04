using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class MembershipPlan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public int DurationInDays { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<UserMembership> UserMemberships { get; set; } = new List<UserMembership>();
}
