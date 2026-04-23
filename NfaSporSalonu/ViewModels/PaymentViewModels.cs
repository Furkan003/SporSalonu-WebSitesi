namespace NfaSporSalonu.ViewModels
{
    public class PaymentListViewModel
    {
        public List<PaymentItemDto> Payments { get; set; } = new();
        public decimal TotalPaid { get; set; }
    }

    public class PaymentItemDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Status { get; set; }
        public string? PlanName { get; set; }
    }

    public class PaymentDetailViewModel
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Status { get; set; }

        // İlişkili bilgiler
        public string? MemberFullName { get; set; }
        public string? MemberEmail { get; set; }
        public string? PlanName { get; set; }
        public int? PlanDuration { get; set; }
        public DateTime? MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
    }

    public class AdminPaymentListViewModel
    {
        public List<PaymentItemDto> Payments { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingCount { get; set; }
        public string? FilterStatus { get; set; }
    }
}
