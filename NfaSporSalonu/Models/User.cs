using System;
using System.Collections.Generic;

namespace NfaSporSalonu.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AccessCredential> AccessCredentials { get; set; } = new List<AccessCredential>();

    public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();

    public virtual ICollection<MemberMeasurement> MemberMeasurements { get; set; } = new List<MemberMeasurement>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TrainerTrainee> TrainerTraineeTrainees { get; set; } = new List<TrainerTrainee>();

    public virtual ICollection<TrainerTrainee> TrainerTraineeTrainers { get; set; } = new List<TrainerTrainee>();

    public virtual ICollection<UserMembership> UserMemberships { get; set; } = new List<UserMembership>();

    public virtual ICollection<WorkoutAndDietProgram> WorkoutAndDietProgramTrainees { get; set; } = new List<WorkoutAndDietProgram>();

    public virtual ICollection<WorkoutAndDietProgram> WorkoutAndDietProgramTrainers { get; set; } = new List<WorkoutAndDietProgram>();
}
