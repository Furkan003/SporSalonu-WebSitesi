using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Services;

public class TurnstileService : ITurnstileService
{
    private readonly NfaSporSalonuDbContext _context;

    public TurnstileService(NfaSporSalonuDbContext context)
    {
        _context = context;
    }

    public async Task<TurnstileVerifyResponse> VerifyAccess(string qrCode)
    {
        var now = DateTime.Now;

        // ─── 1. QR kodu AccessCredential tablosunda bul ───
        var credential = await _context.AccessCredentials
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CredentialValue == qrCode && c.IsActive == true);

        if (credential == null || credential.User == null)
        {
            return new TurnstileVerifyResponse
            {
                IsGranted = false,
                Message = "Geçersiz QR kod — Kayıt bulunamadı.",
                AccessTime = now
            };
        }

        var user = credential.User;
        var memberName = $"{user.FirstName} {user.LastName}";

        // ─── 2. Kullanıcı aktif mi kontrol et ───
        if (user.IsActive != true)
        {
            await LogAccess(user.UserId, false, "QR", "Hesap devre dışı");
            return new TurnstileVerifyResponse
            {
                IsGranted = false,
                Message = "Hesap devre dışı — Giriş reddedildi.",
                MemberName = memberName,
                AccessTime = now
            };
        }

        // ─── 3. Aktif üyelik paketi kontrol et (EndDate) ───
        var activeMembership = await _context.UserMemberships
            .Include(um => um.Plan)
            .Where(um => um.UserId == user.UserId && um.Status == "Active" && um.EndDate > now)
            .OrderByDescending(um => um.EndDate)
            .FirstOrDefaultAsync();

        if (activeMembership == null)
        {
            // Süresi dolmuş veya üyelik yok
            await LogAccess(user.UserId, false, "QR", "Süresi Dolmuş - Reddedildi");
            return new TurnstileVerifyResponse
            {
                IsGranted = false,
                Message = "Süresi Dolmuş - Reddedildi",
                MemberName = memberName,
                AccessTime = now
            };
        }

        // ─── 4. Giriş başarılı ───
        await LogAccess(user.UserId, true, "QR", null);

        return new TurnstileVerifyResponse
        {
            IsGranted = true,
            Message = "Giriş Başarılı",
            MemberName = memberName,
            PlanName = activeMembership.Plan?.PlanName,
            MembershipEndDate = activeMembership.EndDate,
            AccessTime = now
        };
    }

    private async Task LogAccess(int userId, bool isGranted, string method, string? denialReason)
    {
        var log = new AccessLog
        {
            UserId = userId,
            AccessType = "Entry",
            Method = method,
            AccessTime = DateTime.Now,
            IsGranted = isGranted,
            DenialReason = denialReason
        };

        _context.AccessLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
