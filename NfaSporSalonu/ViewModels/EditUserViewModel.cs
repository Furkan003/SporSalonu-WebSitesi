using System.ComponentModel.DataAnnotations;

namespace NfaSporSalonu.ViewModels
{
    /// <summary>
    /// Admin tarafından kullanıcı bilgilerini düzenlemek için (şifre hariç)
    /// </summary>
    public class EditUserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Display(Name = "Telefon")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Cinsiyet")]
        [StringLength(10)]
        public string? Gender { get; set; }

        [Display(Name = "Doğum Tarihi")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Rol")]
        public int? RoleId { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Profil Resmi URL")]
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        // Dropdown için
        public List<RoleSelectItem> AvailableRoles { get; set; } = new();

        // Read-only üyelik bilgisi
        public string? CurrentPlanName { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public bool HasActiveMembership { get; set; }
    }

    /// <summary>
    /// Fatura detay sayfası için ViewModel
    /// </summary>
    public class InvoiceViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        // Üyelik bilgileri
        public string? PlanName { get; set; }
        public decimal? PlanPrice { get; set; }
        public DateTime? MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public string? MembershipStatus { get; set; }
        public int? DurationInDays { get; set; }

        // Fatura meta
        public string InvoiceNumber { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }
    }
}
