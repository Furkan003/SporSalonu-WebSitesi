using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class AccessCredential
{
    public int CredentialId { get; set; }

    public int? UserId { get; set; }

    public string CredentialType { get; set; } = null!;

    public string CredentialValue { get; set; } = null!;

    public DateTime? AssignedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual User? User { get; set; }
}
