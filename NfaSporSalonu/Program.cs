using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using NfaSporSalonu.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<NfaSporSalonuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS Policy
var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Dual Authentication (Cookie + JWT)
builder.Services.AddAuthentication(options =>
{
    // Default scheme is a policy scheme that decides based on the path
    options.DefaultScheme = "DualScheme";
    options.DefaultChallengeScheme = "DualScheme";
})
.AddPolicyScheme("DualScheme", "JWT or Cookie based on path", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Rate Limiting (IP Based for API)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("ApiLoginLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// Register Application Services (Dependency Injection)
builder.Services.AddScoped<NfaSporSalonu.Services.IAnalyticsService, NfaSporSalonu.Services.AnalyticsService>();
builder.Services.AddScoped<NfaSporSalonu.Services.ITurnstileService, NfaSporSalonu.Services.TurnstileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Global Exception Handling for API
app.Use(async (context, next) =>
{
    try
    {
        await next();
        
        // Handle unauthorized API requests properly (preventing 302 redirects)
        if (context.Response.StatusCode == 401 && context.Request.Path.StartsWithSegments("/api"))
        {
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Unauthorized", status = 401 }));
            }
        }
    }
    catch (Exception ex)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { 
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An internal error occurred.",
                status = 500,
                detail = app.Environment.IsDevelopment() ? ex.Message : "A system error occurred."
            }));
        }
        else
        {
            throw; // Let MVC handle it
        }
    }
});

app.UseRouting();
app.UseCors("DefaultCorsPolicy");
app.UseRateLimiter();

// Kimlik doğrulama ve yetkilendirme middleware sırası önemli!
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();