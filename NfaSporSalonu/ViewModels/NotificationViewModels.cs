using System.ComponentModel.DataAnnotations;

namespace NfaSporSalonu.ViewModels
{
    public class SendNotificationViewModel
    {
        [Display(Name = "Gönderim Türü")]
        public bool SendToAll { get; set; }

        [Display(Name = "Üye")]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Mesaj zorunludur.")]
        [Display(Name = "Mesaj")]
        [StringLength(500, ErrorMessage = "Mesaj en fazla 500 karakter olabilir.")]
        public string Message { get; set; } = null!;

        [Display(Name = "Bildirim Türü")]
        [StringLength(20)]
        public string? NotificationType { get; set; }

        // Dropdown listesi
        public List<UserSelectItem> AvailableUsers { get; set; } = new();
    }

    public class AdminNotificationListViewModel
    {
        public List<AdminNotificationItemDto> Notifications { get; set; } = new();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
    }

    public class AdminNotificationItemDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = null!;
        public string? NotificationType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
    }

    public class MemberNotificationListViewModel
    {
        public List<NotificationDto> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
