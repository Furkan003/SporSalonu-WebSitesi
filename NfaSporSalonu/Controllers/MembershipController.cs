using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.ViewModels;

namespace NfaSporSalonu.Controllers
{
    [Authorize]
    public class MembershipController : Controller
    {
        private readonly NfaSporSalonuDbContext _context;

        public MembershipController(NfaSporSalonuDbContext context)
        {
            _context = context;
        }

        // ═══════════════ ÜYE DASHBOARD ═══════════════

        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var now = DateTime.Now;

            // Kullanıcı bilgileri
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // Aktif üyelik
            var activeMembership = await _context.UserMemberships
                .Include(um => um.Plan)
                .Where(um => um.UserId == userId && um.Status == "Active" && um.EndDate > now)
                .OrderByDescending(um => um.EndDate)
                .FirstOrDefaultAsync();

            // Son ölçüm
            var lastMeasurement = await _context.MemberMeasurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.MeasurementDate)
                .FirstOrDefaultAsync();

            // Eğitmen bilgisi (TrainerTrainee ilişkisinden)
            var trainerRelation = await _context.TrainerTrainees
                .Include(tt => tt.Trainer)
                .Where(tt => tt.TraineeId == userId)
                .OrderByDescending(tt => tt.AssignedDate)
                .FirstOrDefaultAsync();

            // Aktif program sayısı
            var activeProgramCount = await _context.WorkoutAndDietPrograms
                .Where(p => p.TraineeId == userId && (p.EndDate == null || p.EndDate > now))
                .CountAsync();

            // Bildirimler
            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false)
                .CountAsync();

            var recentNotifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead ?? false
                })
                .ToListAsync();

            var viewModel = new MemberDashboardViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                CurrentPlanName = activeMembership?.Plan?.PlanName,
                MembershipStartDate = activeMembership?.StartDate,
                MembershipEndDate = activeMembership?.EndDate,
                MembershipStatus = activeMembership?.Status ?? "Üyelik Yok",
                RemainingDays = activeMembership != null
                    ? (int)(activeMembership.EndDate - now).TotalDays
                    : null,
                LastWeight = lastMeasurement?.Weight,
                LastHeight = lastMeasurement?.Height,
                LastMeasurementDate = lastMeasurement?.MeasurementDate,
                ActiveProgramCount = activeProgramCount,
                CurrentTrainerName = trainerRelation?.Trainer != null
                    ? $"{trainerRelation.Trainer.FirstName} {trainerRelation.Trainer.LastName}"
                    : null,
                UnreadNotificationCount = unreadCount,
                RecentNotifications = recentNotifications
            };

            return View(viewModel);
        }

        // ═══════════════ ÜYELİK PAKETLERİ ═══════════════

        public async Task<IActionResult> Packages()
        {
            var userId = GetCurrentUserId();
            var now = DateTime.Now;

            // Aktif üyelik bilgisi
            UserMembership? activeMembership = null;
            if (userId != null)
            {
                activeMembership = await _context.UserMemberships
                    .Include(um => um.Plan)
                    .Where(um => um.UserId == userId && um.Status == "Active" && um.EndDate > now)
                    .OrderByDescending(um => um.EndDate)
                    .FirstOrDefaultAsync();
            }

            var plans = await _context.MembershipPlans
                .Where(p => p.IsActive == true)
                .Select(p => new MembershipPlanDto
                {
                    PlanId = p.PlanId,
                    PlanName = p.PlanName,
                    DurationInDays = p.DurationInDays,
                    Price = p.Price,
                    Description = p.Description,
                    ActiveMemberCount = p.UserMemberships
                        .Count(um => um.Status == "Active" && um.EndDate > now)
                })
                .ToListAsync();

            var viewModel = new MembershipPackagesViewModel
            {
                Plans = plans,
                CurrentPlanName = activeMembership?.Plan?.PlanName,
                MembershipEndDate = activeMembership?.EndDate
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Purchase(int planId)
        {
            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.PlanId == planId && p.IsActive == true);

            if (plan == null)
            {
                TempData["Error"] = "Paket bulunamadı.";
                return RedirectToAction(nameof(Packages));
            }

            var model = new PurchaseMembershipViewModel
            {
                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                Price = plan.Price,
                DurationInDays = plan.DurationInDays,
                Description = plan.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(PurchaseMembershipViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var plan = await _context.MembershipPlans.FindAsync(model.PlanId);
            if (plan == null)
            {
                TempData["Error"] = "Paket bulunamadı.";
                return RedirectToAction(nameof(Packages));
            }

            var now = DateTime.Now;

            // UserMembership oluştur
            var membership = new UserMembership
            {
                UserId = userId,
                PlanId = plan.PlanId,
                StartDate = now,
                EndDate = now.AddDays(plan.DurationInDays),
                PurchaseDate = now,
                Status = "Active"
            };

            _context.UserMemberships.Add(membership);
            await _context.SaveChangesAsync();

            // Payment kaydı oluştur
            var payment = new Payment
            {
                UserId = userId,
                UserMembershipId = membership.UserMembershipId,
                Amount = plan.Price,
                PaymentMethod = model.PaymentMethod,
                TransactionId = Guid.NewGuid().ToString("N")[..12].ToUpper(),
                PaymentDate = now,
                Status = "Completed"
            };

            _context.Payments.Add(payment);

            // Bildirim gönder
            var notification = new Notification
            {
                UserId = userId,
                Message = $"{plan.PlanName} paketi başarıyla satın alındı. Bitiş: {membership.EndDate:dd.MM.yyyy}",
                NotificationType = "Üyelik",
                CreatedDate = now,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{plan.PlanName} paketi başarıyla satın alındı!";
            return RedirectToAction(nameof(Dashboard));
        }

        // ═══════════════ ÖDEME GEÇMİŞİ ═══════════════

        public async Task<IActionResult> Payments()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var payments = await _context.Payments
                .Include(p => p.UserMembership)
                    .ThenInclude(um => um!.Plan)
                .Where(p => p.UserId == userId)
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

            var viewModel = new PaymentListViewModel
            {
                Payments = payments,
                TotalPaid = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PaymentDetail(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.UserMembership)
                    .ThenInclude(um => um!.Plan)
                .FirstOrDefaultAsync(p => p.PaymentId == id && p.UserId == userId);

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

        // ═══════════════ ÖLÇÜM TAKİBİ ═══════════════

        public async Task<IActionResult> Measurements()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var measurements = await _context.MemberMeasurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.MeasurementDate)
                .Select(m => new MeasurementItemDto
                {
                    MeasurementId = m.MeasurementId,
                    MeasurementDate = m.MeasurementDate,
                    Height = m.Height,
                    Weight = m.Weight,
                    Bicep = m.Bicep,
                    Chest = m.Chest,
                    Waist = m.Waist,
                    Notes = m.Notes
                })
                .ToListAsync();

            var viewModel = new MeasurementListViewModel { Measurements = measurements };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddMeasurement()
        {
            return View(new AddMeasurementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeasurement(AddMeasurementViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var measurement = new MemberMeasurement
            {
                UserId = userId,
                Height = model.Height,
                Weight = model.Weight,
                Bicep = model.Bicep,
                Chest = model.Chest,
                Waist = model.Waist,
                Notes = model.Notes,
                MeasurementDate = DateTime.Now
            };

            _context.MemberMeasurements.Add(measurement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ölçüm başarıyla kaydedildi.";
            return RedirectToAction(nameof(Measurements));
        }

        // ═══════════════ PROGRAMLARIM ═══════════════

        public async Task<IActionResult> Programs()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var programs = await _context.WorkoutAndDietPrograms
                .Include(p => p.Trainer)
                .Where(p => p.TraineeId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new MemberProgramItemDto
                {
                    ProgramId = p.ProgramId,
                    Title = p.Title,
                    ProgramType = p.ProgramType,
                    TrainerName = p.Trainer != null ? p.Trainer.FirstName + " " + p.Trainer.LastName : null,
                    CreatedDate = p.CreatedDate,
                    EndDate = p.EndDate
                })
                .ToListAsync();

            // Eğitmen bilgisi
            var trainerRelation = await _context.TrainerTrainees
                .Include(tt => tt.Trainer)
                .Where(tt => tt.TraineeId == userId)
                .OrderByDescending(tt => tt.AssignedDate)
                .FirstOrDefaultAsync();

            var viewModel = new MemberProgramListViewModel
            {
                Programs = programs,
                TrainerName = trainerRelation?.Trainer != null
                    ? $"{trainerRelation.Trainer.FirstName} {trainerRelation.Trainer.LastName}"
                    : null
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ProgramDetail(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var program = await _context.WorkoutAndDietPrograms
                .Include(p => p.Trainer)
                .FirstOrDefaultAsync(p => p.ProgramId == id && p.TraineeId == userId);

            if (program == null)
            {
                TempData["Error"] = "Program bulunamadı.";
                return RedirectToAction(nameof(Programs));
            }

            var viewModel = new ProgramDetailViewModel
            {
                ProgramId = program.ProgramId,
                Title = program.Title,
                ProgramType = program.ProgramType,
                Content = program.Content,
                TrainerName = program.Trainer != null ? $"{program.Trainer.FirstName} {program.Trainer.LastName}" : null,
                CreatedDate = program.CreatedDate,
                EndDate = program.EndDate
            };

            return View(viewModel);
        }

        // ═══════════════ BİLDİRİMLER ═══════════════

        public async Task<IActionResult> Notifications()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead ?? false
                })
                .ToListAsync();

            var viewModel = new MemberNotificationListViewModel
            {
                Notifications = notifications,
                UnreadCount = notifications.Count(n => !n.IsRead)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Tüm bildirimler okundu olarak işaretlendi.";
            return RedirectToAction(nameof(Notifications));
        }

        // ═══════════════ YARDIMCI METOT ═══════════════

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return null;
        }
    }
}
