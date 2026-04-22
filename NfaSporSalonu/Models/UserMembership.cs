using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class UserMembership
{
    public int UserMembershipId { get; set; }

    public int? UserId { get; set; }

    public int? PlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual MembershipPlan? Plan { get; set; }

    public virtual User? User { get; set; }
}
