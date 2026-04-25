namespace NfaSporSalonu.ViewModels
{
    public class MemberDashboardViewModel
    {
        // Üye Bilgileri
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }

        // Aktif Üyelik Bilgisi
        public string? CurrentPlanName { get; set; }
        public DateTime? MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public string? MembershipStatus { get; set; }
        public int? RemainingDays { get; set; }

        // Son Ölçümler
        public decimal? LastWeight { get; set; }
        public decimal? LastHeight { get; set; }
        public DateTime? LastMeasurementDate { get; set; }

        // Program Bilgileri
        public int ActiveProgramCount { get; set; }
        public string? CurrentTrainerName { get; set; }

        // Bildirimler
        public int UnreadNotificationCount { get; set; }
        public List<NotificationDto> RecentNotifications { get; set; } = new();

        // Eğitmen Notları
        public List<TrainerNoteDto> TrainerNotes { get; set; } = new();
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = null!;
        public string? NotificationType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class TrainerNoteDto
    {
        public string TrainerName { get; set; } = null!;
        public string NoteContent { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
