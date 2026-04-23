using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.ViewModels;

namespace NfaSporSalonu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly NfaSporSalonuDbContext _context;

        public AdminController(NfaSporSalonuDbContext context)
        {
            _context = context;
        }

        // ───────────── DASHBOARD ─────────────

        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Genel İstatistikler
            var totalUsers = await _context.Users.CountAsync();
            var activeMembers = await _context.Users
                .Where(u => u.IsActive == true && u.Role != null && u.Role.RoleName == "Member")
                .CountAsync();
            var totalTrainers = await _context.Users
                .Where(u => u.Role != null && u.Role.RoleName == "Trainer")
                .CountAsync();

            // Üyelik İstatistikleri
            var activeMemberships = await _context.UserMemberships
                .Where(um => um.Status == "Active" && um.EndDate > now)
                .CountAsync();
            var expiredMemberships = await _context.UserMemberships
                .Where(um => um.Status == "Expired" || um.EndDate <= now)
                .CountAsync();
            var totalPlans = await _context.MembershipPlans
                .Where(p => p.IsActive == true)
                .CountAsync();

            // Finansal İstatistikler
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var monthlyRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" && p.PaymentDate >= startOfMonth)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var pendingPayments = await _context.Payments
                .Where(p => p.Status == "Pending")
                .CountAsync();

            // Erişim & Bildirim
            var todayStart = now.Date;
            var todayAccessCount = await _context.AccessLogs
                .Where(a => a.AccessTime >= todayStart)
                .CountAsync();
            var unreadNotifications = await _context.Notifications
                .Where(n => n.IsRead == false)
                .CountAsync();

            // Son Kayıt Olan 5 Üye
            var recentMembers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentMemberDto
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    RoleName = u.Role != null ? u.Role.RoleName : null
                })
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveMembers = activeMembers,
                TotalTrainers = totalTrainers,
                ActiveMemberships = activeMemberships,
                ExpiredMemberships = expiredMemberships,
                TotalMembershipPlans = totalPlans,
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                PendingPayments = pendingPayments,
                TodayAccessCount = todayAccessCount,
                UnreadNotifications = unreadNotifications,
                RecentMembers = recentMembers
            };

            return View(viewModel);
        }
    }
}
