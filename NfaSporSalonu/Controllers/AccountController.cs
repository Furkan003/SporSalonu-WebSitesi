using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.ViewModels;

namespace NfaSporSalonu.Controllers
{
    public class AccountController : Controller
    {
        private readonly NfaSporSalonuDbContext _context;

        public AccountController(NfaSporSalonuDbContext context)
        {
            _context = context;
        }

        // ───────────── GİRİŞ YAP ─────────────

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcıyı e-posta ile bul (rol bilgisini de çek)
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // Kullanıcı bulunamadı
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(model);
            }

            // Brute-force kilidi kontrolü
            if (user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.Now)
            {
                var remaining = (int)(user.LockoutEndTime.Value - DateTime.Now).TotalMinutes;
                ModelState.AddModelError(string.Empty, $"Hesabınız kilitli. {remaining + 1} dakika sonra tekrar deneyin.");
                return View(model);
            }

            // Şifre doğrulama
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                // Hatalı giriş sayacını artır
                user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEndTime = DateTime.Now.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                    await _context.SaveChangesAsync();
                    ModelState.AddModelError(string.Empty, "Çok fazla hatalı giriş. Hesabınız 15 dakika kilitlendi.");
                    return View(model);
                }

                await _context.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(model);
            }

            // Hesap aktif değilse girişe izin verme
            if (user.IsActive == false)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız devre dışı bırakılmıştır. Lütfen yönetici ile iletişime geçin.");
                return View(model);
            }

            // Başarılı giriş — sayacı sıfırla
            user.FailedLoginAttempts = 0;
            user.LockoutEndTime = null;
            await _context.SaveChangesAsync();

            // Claim'leri oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
            };

            // Rol bilgisi varsa claim'e ekle
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            TempData["Success"] = "Giriş başarılı! Hoş geldiniz.";

            // returnUrl varsa oraya, yoksa role göre yönlendir
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.Role?.RoleName == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Dashboard", "Membership");
        }

        // ───────────── KAYIT OL ─────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // E-posta daha önce kayıtlı mı kontrol et
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (existingUser)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(model);
            }

            // "Member" rolünü bul (yoksa default olarak null bırakılır)
            var memberRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Member");

            // Yeni kullanıcı oluştur (Entity'ye ViewModel'den map'le)
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                RoleId = memberRole?.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        // ───────────── ÇIKIŞ YAP ─────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Başarıyla çıkış yapıldı.";
            return RedirectToAction("Index", "Home");
        }

        // ───────────── ERİŞİM ENGELLENDİ ─────────────

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
