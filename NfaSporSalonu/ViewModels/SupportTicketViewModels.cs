using System.ComponentModel.DataAnnotations;

namespace NfaSporSalonu.ViewModels
{
    public class CreateSupportTicketViewModel
    {
        [Required(ErrorMessage = "Kategori seçiniz.")]
        [Display(Name = "Kategori")]
        public string Category { get; set; } = null!;

        [Required(ErrorMessage = "Konu alanı zorunludur.")]
        [Display(Name = "Konu")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir.")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [Display(Name = "Mesajınız")]
        [StringLength(2000, ErrorMessage = "Mesaj en fazla 2000 karakter olabilir.")]
        public string Message { get; set; } = null!;
    }

    public class SupportTicketItemDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? AdminResponse { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
    }

    public class AdminSupportTicketListViewModel
    {
        public List<SupportTicketItemDto> Tickets { get; set; } = new();
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class RespondTicketViewModel
    {
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Yanıt alanı zorunludur.")]
        [Display(Name = "Yanıtınız")]
        [StringLength(2000)]
        public string AdminResponse { get; set; } = null!;

        public string? NewStatus { get; set; } = "Yanıtlandı";
    }
}
