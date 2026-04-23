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

        // ═══════════════ DASHBOARD ═══════════════

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

        // ═══════════════ ÖDEME YÖNETİMİ ═══════════════

        public async Task<IActionResult> Payments(string? status = null)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var query = _context.Payments
                .Include(p => p.User)
                .Include(p => p.UserMembership)
                    .ThenInclude(um => um!.Plan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentItemDto
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    PlanName = p.UserMembership != null && p.UserMembership.Plan != null
                        ? p.UserMembership.Plan.PlanName
                        : null
                })
                .ToListAsync();

            var viewModel = new AdminPaymentListViewModel
            {
                Payments = payments,
                TotalRevenue = await _context.Payments
                    .Where(p => p.Status == "Completed")
                    .SumAsync(p => (decimal?)p.Amount) ?? 0,
                MonthlyRevenue = await _context.Payments
                    .Where(p => p.Status == "Completed" && p.PaymentDate >= startOfMonth)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0,
                PendingCount = await _context.Payments.CountAsync(p => p.Status == "Pending"),
                FilterStatus = status
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PaymentDetail(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.UserMembership)
                    .ThenInclude(um => um!.Plan)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                TempData["Error"] = "Ödeme bulunamadı.";
                return RedirectToAction(nameof(Payments));
            }

            var viewModel = new PaymentDetailViewModel
            {
                PaymentId = payment.PaymentId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status,
                MemberFullName = payment.User != null ? $"{payment.User.FirstName} {payment.User.LastName}" : null,
                MemberEmail = payment.User?.Email,
                PlanName = payment.UserMembership?.Plan?.PlanName,
                PlanDuration = payment.UserMembership?.Plan?.DurationInDays,
                MembershipStartDate = payment.UserMembership?.StartDate,
                MembershipEndDate = payment.UserMembership?.EndDate
            };

            return View(viewModel);
        }

        // ═══════════════ BİLDİRİM YÖNETİMİ ═══════════════

        public async Task<IActionResult> Notifications()
        {
            var notifications = await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedDate)
                .Take(100)
                .Select(n => new AdminNotificationItemDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead ?? false,
                    UserFullName = n.User != null ? n.User.FirstName + " " + n.User.LastName : null,
                    UserEmail = n.User != null ? n.User.Email : null
                })
                .ToListAsync();

            var viewModel = new AdminNotificationListViewModel
            {
                Notifications = notifications,
                TotalCount = await _context.Notifications.CountAsync(),
                UnreadCount = await _context.Notifications.CountAsync(n => n.IsRead == false)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SendNotification()
        {
            var model = new SendNotificationViewModel
            {
                AvailableUsers = await _context.Users
                    .Where(u => u.IsActive == true)
                    .Select(u => new UserSelectItem
                    {
                        UserId = u.UserId,
                        FullName = u.FirstName + " " + u.LastName,
                        Email = u.Email
                    })
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(SendNotificationViewModel model)
        {
            if (!model.SendToAll && model.UserId == null)
            {
                ModelState.AddModelError("UserId", "Bir üye seçiniz veya 'Tüm Üyelere Gönder' seçeneğini işaretleyiniz.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _context.Users
                    .Where(u => u.IsActive == true)
                    .Select(u => new UserSelectItem
                    {
                        UserId = u.UserId,
                        FullName = u.FirstName + " " + u.LastName,
                        Email = u.Email
                    })
                    .ToListAsync();
                return View(model);
            }

            var now = DateTime.Now;

            if (model.SendToAll)
            {
                // Tüm aktif üyelere gönder
                var userIds = await _context.Users
                    .Where(u => u.IsActive == true)
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var uid in userIds)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = uid,
                        Message = model.Message,
                        NotificationType = model.NotificationType,
                        CreatedDate = now,
                        IsRead = false
                    });
                }

                TempData["Success"] = $"{userIds.Count} üyeye bildirim gönderildi.";
            }
            else
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = model.UserId,
                    Message = model.Message,
                    NotificationType = model.NotificationType,
                    CreatedDate = now,
                    IsRead = false
                });

                TempData["Success"] = "Bildirim başarıyla gönderildi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                TempData["Error"] = "Bildirim bulunamadı.";
                return RedirectToAction(nameof(Notifications));
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bildirim silindi.";
            return RedirectToAction(nameof(Notifications));
        }
    }
}
