using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class WorkoutAndDietProgram
{
    public int ProgramId { get; set; }

    public int? TrainerId { get; set; }

    public int? TraineeId { get; set; }

    public string ProgramType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual User? Trainee { get; set; }

    public virtual User? Trainer { get; set; }
}
