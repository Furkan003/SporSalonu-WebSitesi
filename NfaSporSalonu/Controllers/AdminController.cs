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

            // Son 30 Gündeki Kayıtlar
            var thirtyDaysAgo = now.AddDays(-30);
            var last30DaysRegistrations = await _context.Users
                .Where(u => u.CreatedAt >= thirtyDaysAgo)
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
                Last30DaysRegistrations = last30DaysRegistrations,
                RecentMembers = recentMembers
            };

            return View(viewModel);
        }

        // ═══════════════ KULLANICI YÖNETİMİ ═══════════════

        public async Task<IActionResult> Users()
        {
            var now = DateTime.Now;
            var roles = await _context.Roles
                .Select(r => new RoleSelectItem { RoleId = r.RoleId, RoleName = r.RoleName })
                .ToListAsync();

            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserMemberships)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserItemDto
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.Role != null ? u.Role.RoleName : null,
                    RoleId = u.RoleId,
                    IsActive = u.IsActive == true,
                    CreatedAt = u.CreatedAt,
                    HasActiveMembership = u.UserMemberships.Any(um => um.Status == "Active" && um.EndDate > now),
                    MembershipEndDate = u.UserMemberships
                        .Where(um => um.Status == "Active" && um.EndDate > now)
                        .OrderByDescending(um => um.EndDate)
                        .Select(um => (DateTime?)um.EndDate)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var viewModel = new AdminUserListViewModel
            {
                Users = users,
                AvailableRoles = roles,
                TotalCount = users.Count,
                ActiveCount = users.Count(u => u.IsActive)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            var role = await _context.Roles.FindAsync(model.NewRoleId);
            if (role == null)
            {
                TempData["Error"] = "Seçilen rol bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            user.RoleId = model.NewRoleId;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{user.FirstName} {user.LastName} kullanıcısının rolü \"{role.RoleName}\" olarak güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        // ═══════════════ KULLANICI DÜZENLEME ═══════════════

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var now = DateTime.Now;
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Plan)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            var activeMembership = user.UserMemberships
                .Where(um => um.Status == "Active" && um.EndDate > now)
                .OrderByDescending(um => um.EndDate)
                .FirstOrDefault();

            var roles = await _context.Roles
                .Select(r => new RoleSelectItem { RoleId = r.RoleId, RoleName = r.RoleName })
                .ToListAsync();

            var viewModel = new EditUserViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                RoleId = user.RoleId,
                IsActive = user.IsActive == true,
                ProfileImageUrl = user.ProfileImageUrl,
                AvailableRoles = roles,
                CurrentPlanName = activeMembership?.Plan?.PlanName,
                MembershipEndDate = activeMembership?.EndDate,
                HasActiveMembership = activeMembership != null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _context.Roles
                    .Select(r => new RoleSelectItem { RoleId = r.RoleId, RoleName = r.RoleName })
                    .ToListAsync();
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            // E-posta benzersizlik kontrolü (kendi kaydı hariç)
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
                model.AvailableRoles = await _context.Roles
                    .Select(r => new RoleSelectItem { RoleId = r.RoleId, RoleName = r.RoleName })
                    .ToListAsync();
                return View(model);
            }

            // Güncelle (şifre hariç)
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            user.ProfileImageUrl = model.ProfileImageUrl;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{user.FirstName} {user.LastName} kullanıcısı başarıyla güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        // ═══════════════ FATURA KESİMİ ═══════════════

        [HttpGet]
        public async Task<IActionResult> Invoice(int userId)
        {
            var now = DateTime.Now;
            var user = await _context.Users
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Plan)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            var activeMembership = user.UserMemberships
                .Where(um => um.Status == "Active" && um.EndDate > now)
                .OrderByDescending(um => um.EndDate)
                .FirstOrDefault();

            var viewModel = new InvoiceViewModel
            {
                UserId = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PlanName = activeMembership?.Plan?.PlanName,
                PlanPrice = activeMembership?.Plan?.Price,
                MembershipStartDate = activeMembership?.StartDate,
                MembershipEndDate = activeMembership?.EndDate,
                MembershipStatus = activeMembership != null ? "Aktif" : "Üyelik Yok",
                DurationInDays = activeMembership?.Plan?.DurationInDays,
                InvoiceNumber = $"NFA-{now:yyyyMMdd}-{userId:D4}",
                InvoiceDate = now
            };

            return View(viewModel);
        }

        // ═══════════════ ÜYELİK ATAMA ═══════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantMembership(GrantMembershipViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            if (model.DurationMonths != 1 && model.DurationMonths != 3)
            {
                TempData["Error"] = "Geçersiz üyelik süresi.";
                return RedirectToAction(nameof(Users));
            }

            var now = DateTime.Now;

            // Mevcut aktif üyelik var mı? Varsa onun EndDate'ine ekle, yoksa bugünden başla
            var existingMembership = await _context.UserMemberships
                .Where(um => um.UserId == model.UserId && um.Status == "Active" && um.EndDate > now)
                .OrderByDescending(um => um.EndDate)
                .FirstOrDefaultAsync();

            if (existingMembership != null)
            {
                // Mevcut üyeliğin bitiş tarihine ekle
                existingMembership.EndDate = existingMembership.EndDate.AddMonths(model.DurationMonths);
                _context.UserMemberships.Update(existingMembership);
            }
            else
            {
                // Yeni üyelik oluştur
                var newMembership = new UserMembership
                {
                    UserId = model.UserId,
                    StartDate = now,
                    EndDate = now.AddMonths(model.DurationMonths),
                    PurchaseDate = now,
                    Status = "Active"
                };
                _context.UserMemberships.Add(newMembership);
            }

            // Otomatik bildirim gönder
            var durationText = model.DurationMonths == 1 ? "1 Aylık" : "3 Aylık";
            _context.Notifications.Add(new Notification
            {
                UserId = model.UserId,
                Message = $"🎉 Tebrikler! Size {durationText} üyelik tanımlandı. Spor salonumuza hoş geldiniz!",
                NotificationType = "Membership",
                CreatedDate = now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{user.FirstName} {user.LastName} kullanıcısına {durationText} üyelik başarıyla tanımlandı.";
            return RedirectToAction(nameof(Users));
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
