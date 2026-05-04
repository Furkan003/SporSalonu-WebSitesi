using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace NfaSporSalonu.Filters;

/// <summary>
/// IDOR (Insecure Direct Object Reference) koruması.
/// Admin her şeyi görebilir.
/// Trainer sadece kendi trainee'lerine ait kaynaklara erişebilir.
/// Member sadece kendi kaynaklarına erişebilir.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ResourceOwnerAuthorizationFilter : Attribute, IAsyncActionFilter
{
    private readonly string _userIdRouteParam;
    private readonly bool _allowTrainerAccess;

    /// <param name="userIdRouteParam">Route veya query'deki kullanıcı ID parametre adı (varsayılan: "id")</param>
    /// <param name="allowTrainerAccess">Trainer rolüne de erişim izni verilsin mi</param>
    public ResourceOwnerAuthorizationFilter(string userIdRouteParam = "id", bool allowTrainerAccess = false)
    {
        _userIdRouteParam = userIdRouteParam;
        _allowTrainerAccess = allowTrainerAccess;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        // Kimlik doğrulaması yoksa 401
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var currentUserIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(currentUserIdStr, out var currentUserId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var role = user.FindFirstValue(ClaimTypes.Role);

        // Admin her şeye erişebilir
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        // Route'tan hedef resource ID'sini al
        if (!context.ActionArguments.TryGetValue(_userIdRouteParam, out var targetIdObj) &&
            !context.RouteData.Values.TryGetValue(_userIdRouteParam, out targetIdObj))
        {
            // Parametre yoksa devam et (controller kendi kontrol etsin)
            await next();
            return;
        }

        if (!int.TryParse(targetIdObj?.ToString(), out var targetUserId))
        {
            await next();
            return;
        }

        // Trainer erişim kontrolü
        if (_allowTrainerAccess && string.Equals(role, "Trainer", StringComparison.OrdinalIgnoreCase))
        {
            var db = httpContext.RequestServices
                .GetRequiredService<NfaSporSalonu.Models.NfaSporSalonuDbContext>();

            var isAssigned = await db.TrainerTrainees
                .AnyAsync(tt => tt.TrainerId == currentUserId && tt.TraineeId == targetUserId);

            if (isAssigned)
            {
                await next();
                return;
            }
        }

        // Kaynak sahibi kontrolü — kendi kaynağı mı?
        if (currentUserId == targetUserId)
        {
            await next();
            return;
        }

        // Yatay yetki sömürüsü engellendi
        context.Result = new ObjectResult(new { message = "Bu kaynağa erişim yetkiniz bulunmamaktadır." })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
