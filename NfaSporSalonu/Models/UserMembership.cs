using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// Üyeliğin şu an geçerli olup olmadığını hesaplar.
    /// Tarih aralığı ve Status kontrolü ile belirlenir.
    /// </summary>
    [NotMapped]
    public bool IsActive
    {
        get
        {
            var now = DateTime.Now;
            return Status == "Active"
                && now >= StartDate
                && now <= EndDate;
        }
    }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual MembershipPlan? Plan { get; set; }

    public virtual User? User { get; set; }
}
