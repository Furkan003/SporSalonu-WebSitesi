using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class TrainerTrainee
{
    public int RelationId { get; set; }

    public int? TrainerId { get; set; }

    public int? TraineeId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public string? Notes { get; set; }

    public virtual User? Trainee { get; set; }

    public virtual User? Trainer { get; set; }
}
