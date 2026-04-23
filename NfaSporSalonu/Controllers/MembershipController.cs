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

        // ───────────── ÜYE DASHBOARD ─────────────

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

        // ───────────── YARDIMCI METOT ─────────────

        /// <summary>
        /// Oturum açmış kullanıcının UserId'sini Claim'lerden çeker.
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return null;
        }
    }
}
