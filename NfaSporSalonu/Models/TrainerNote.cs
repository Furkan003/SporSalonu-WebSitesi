using System;

namespace NfaSporSalonu.Models;

public partial class TrainerNote
{
    public int Id { get; set; }

    public int? TrainerId { get; set; }

    public int? MemberId { get; set; }

    public string NoteContent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User? Trainer { get; set; }

    public virtual User? Member { get; set; }
}
