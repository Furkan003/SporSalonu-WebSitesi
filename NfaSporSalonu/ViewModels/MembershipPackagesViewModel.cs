using System.ComponentModel.DataAnnotations;
using NfaSporSalonu.Models;

namespace NfaSporSalonu.ViewModels
{
    public class MembershipPackagesViewModel
    {
        public List<MembershipPlanDto> Plans { get; set; } = new();
        public string? CurrentPlanName { get; set; }
        public DateTime? MembershipEndDate { get; set; }
    }

    public class MembershipPlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public int DurationInDays { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int ActiveMemberCount { get; set; }
    }

    public class PurchaseMembershipViewModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ödeme yöntemi seçiniz.")]
        [Display(Name = "Ödeme Yöntemi")]
        public string PaymentMethod { get; set; } = null!;
    }
}
