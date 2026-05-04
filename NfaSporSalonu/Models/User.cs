using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

    /// <summary>
    /// Bu kullanıcının admin olarak gerçekleştirdiği işlem logları.
    /// </summary>
    public virtual ICollection<AdminActivityLog> AdminActivityLogsAsAdmin { get; set; } = new List<AdminActivityLog>();

    /// <summary>
    /// Bu kullanıcının hedef üye olarak yer aldığı işlem logları.
    /// </summary>
    public virtual ICollection<AdminActivityLog> AdminActivityLogsAsTarget { get; set; } = new List<AdminActivityLog>();

    public virtual ICollection<MemberMeasurement> MemberMeasurements { get; set; } = new List<MemberMeasurement>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TrainerTrainee> TrainerTraineeTrainees { get; set; } = new List<TrainerTrainee>();

    public virtual ICollection<TrainerTrainee> TrainerTraineeTrainers { get; set; } = new List<TrainerTrainee>();

    public virtual ICollection<UserMembership> UserMemberships { get; set; } = new List<UserMembership>();

    public virtual ICollection<WorkoutAndDietProgram> WorkoutAndDietProgramTrainees { get; set; } = new List<WorkoutAndDietProgram>();

    public virtual ICollection<WorkoutAndDietProgram> WorkoutAndDietProgramTrainers { get; set; } = new List<WorkoutAndDietProgram>();

    /// <summary>
    /// Kullanıcının aktif bir üyeliği olup olmadığını kontrol eder.
    /// </summary>
    [NotMapped]
    public bool HasActiveSubscription =>
        UserMemberships?.Any(m => m.IsActive) ?? false;
}
