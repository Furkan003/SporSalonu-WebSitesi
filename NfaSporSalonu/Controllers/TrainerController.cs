using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.ViewModels;

namespace NfaSporSalonu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainerController : Controller
    {
        private readonly NfaSporSalonuDbContext _context;

        public TrainerController(NfaSporSalonuDbContext context)
        {
            _context = context;
        }

        // ───────────── ANTRENÖR LİSTESİ ─────────────

        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Trainer" && u.IsActive == true)
                .Select(u => new TrainerDto
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    TraineeCount = u.TrainerTraineeTrainers.Count,
                    ProgramCount = u.WorkoutAndDietProgramTrainers.Count
                })
                .ToListAsync();

            var viewModel = new TrainerListViewModel { Trainers = trainers };
            return View(viewModel);
        }

        // ───────────── ANTRENÖR ATAMA ─────────────

        [HttpGet]
        public async Task<IActionResult> Assign()
        {
            var model = new AssignTrainerViewModel
            {
                AvailableTrainees = await GetMembersList(),
                AvailableTrainers = await GetTrainersList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTrainees = await GetMembersList();
                model.AvailableTrainers = await GetTrainersList();
                return View(model);
            }

            // Mevcut atama var mı kontrol et
            var existing = await _context.TrainerTrainees
                .AnyAsync(tt => tt.TraineeId == model.TraineeId && tt.TrainerId == model.TrainerId);

            if (existing)
            {
                ModelState.AddModelError("", "Bu üye zaten bu antrenöre atanmış.");
                model.AvailableTrainees = await GetMembersList();
                model.AvailableTrainers = await GetTrainersList();
                return View(model);
            }

            var relation = new TrainerTrainee
            {
                TrainerId = model.TrainerId,
                TraineeId = model.TraineeId,
                Notes = model.Notes,
                AssignedDate = DateTime.Now
            };

            _context.TrainerTrainees.Add(relation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Antrenör ataması başarıyla yapıldı.";
            return RedirectToAction(nameof(Index));
        }

        // ───────────── PROGRAM LİSTESİ ─────────────

        public async Task<IActionResult> Programs()
        {
            var programs = await _context.WorkoutAndDietPrograms
                .Include(p => p.Trainer)
                .Include(p => p.Trainee)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new ProgramItemDto
                {
                    ProgramId = p.ProgramId,
                    Title = p.Title,
                    ProgramType = p.ProgramType,
                    TrainerName = p.Trainer != null ? p.Trainer.FirstName + " " + p.Trainer.LastName : null,
                    TraineeName = p.Trainee != null ? p.Trainee.FirstName + " " + p.Trainee.LastName : null,
                    CreatedDate = p.CreatedDate,
                    EndDate = p.EndDate
                })
                .ToListAsync();

            var viewModel = new ProgramListViewModel { Programs = programs };
            return View(viewModel);
        }

        // ───────────── PROGRAM OLUŞTUR ─────────────

        [HttpGet]
        public async Task<IActionResult> CreateProgram()
        {
            var model = new CreateProgramViewModel
            {
                AvailableTrainees = await GetMembersList(),
                AvailableTrainers = await GetTrainersList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProgram(CreateProgramViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTrainees = await GetMembersList();
                model.AvailableTrainers = await GetTrainersList();
                return View(model);
            }

            var program = new WorkoutAndDietProgram
            {
                TrainerId = model.TrainerId,
                TraineeId = model.TraineeId,
                ProgramType = model.ProgramType,
                Title = model.Title,
                Content = model.Content,
                EndDate = model.EndDate,
                CreatedDate = DateTime.Now
            };

            _context.WorkoutAndDietPrograms.Add(program);
            await _context.SaveChangesAsync();

            // Üyeye bildirim gönder
            var notification = new Notification
            {
                UserId = model.TraineeId,
                Message = $"Yeni bir {model.ProgramType} programı oluşturuldu: {model.Title}",
                NotificationType = "Program",
                CreatedDate = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Program başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Programs));
        }

        // ───────────── PROGRAM SİL ─────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            var program = await _context.WorkoutAndDietPrograms.FindAsync(id);
            if (program == null)
            {
                TempData["Error"] = "Program bulunamadı.";
                return RedirectToAction(nameof(Programs));
            }

            _context.WorkoutAndDietPrograms.Remove(program);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Program başarıyla silindi.";
            return RedirectToAction(nameof(Programs));
        }

        // ───────────── ATAMA SİL ─────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int id)
        {
            var relation = await _context.TrainerTrainees.FindAsync(id);
            if (relation == null)
            {
                TempData["Error"] = "Atama bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            _context.TrainerTrainees.Remove(relation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Atama başarıyla kaldırıldı.";
            return RedirectToAction(nameof(Index));
        }

        // ───────────── YARDIMCI METOTLAR ─────────────

        private async Task<List<UserSelectItem>> GetMembersList()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive == true && u.Role != null && u.Role.RoleName == "Member")
                .Select(u => new UserSelectItem
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToListAsync();
        }

        private async Task<List<UserSelectItem>> GetTrainersList()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive == true && u.Role != null && u.Role.RoleName == "Trainer")
                .Select(u => new UserSelectItem
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToListAsync();
        }
    }
}
