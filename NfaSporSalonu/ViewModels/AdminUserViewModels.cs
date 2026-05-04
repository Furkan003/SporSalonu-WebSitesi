namespace NfaSporSalonu.ViewModels
{
    /// <summary>
    /// Kullanıcı Yönetim Sayfası – Ana ViewModel
    /// </summary>
    public class AdminUserListViewModel
    {
        public List<AdminUserItemDto> Users { get; set; } = new();
        public List<RoleSelectItem> AvailableRoles { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
    }

    /// <summary>
    /// Tabloda gösterilecek kullanıcı satırı
    /// </summary>
    public class AdminUserItemDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? RoleName { get; set; }
        public int? RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Üyelik bilgisi
        public bool HasActiveMembership { get; set; }
        public DateTime? MembershipEndDate { get; set; }
    }

    /// <summary>
    /// Rol dropdown listesi için
    /// </summary>
    public class RoleSelectItem
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

    /// <summary>
    /// Rol değiştirme formu
    /// </summary>
    public class ChangeRoleViewModel
    {
        public int UserId { get; set; }
        public int NewRoleId { get; set; }
    }

    /// <summary>
    /// Üyelik tanımlama formu
    /// </summary>
    public class GrantMembershipViewModel
    {
        public int UserId { get; set; }
        /// <summary>Ay cinsinden süre: 1 veya 3</summary>
        public int DurationMonths { get; set; }
    }
}
