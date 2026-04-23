namespace NfaSporSalonu.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Genel İstatistikler
        public int TotalUsers { get; set; }
        public int ActiveMembers { get; set; }
        public int TotalTrainers { get; set; }

        // Üyelik İstatistikleri
        public int ActiveMemberships { get; set; }
        public int ExpiredMemberships { get; set; }
        public int TotalMembershipPlans { get; set; }

        // Finansal İstatistikler
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingPayments { get; set; }

        // Erişim İstatistikleri
        public int TodayAccessCount { get; set; }
        public int UnreadNotifications { get; set; }

        // Son Kayıt Olan Üyeler
        public List<RecentMemberDto> RecentMembers { get; set; } = new();
    }

    public class RecentMemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string? RoleName { get; set; }
    }
}
