using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? UserId { get; set; }

    public int? UserMembershipId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Status { get; set; }

    public virtual User? User { get; set; }

    public virtual UserMembership? UserMembership { get; set; }
}
