using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.Reports;
using Reflect.Web.Repositories;
using Reflect.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IPatientRepository, InMemoryPatientRepository>();
builder.Services.AddSingleton<IPatientSessionRepository, InMemoryPatientSessionRepository>();
builder.Services.AddSingleton<IClinicalObservationRepository, InMemoryClinicalObservationRepository>();
builder.Services.AddSingleton<IMonitoringDraftRepository, InMemoryMonitoringDraftRepository>();
builder.Services.AddSingleton<ISummaryGenerationService, TemplateSummaryGenerationService>();
builder.Services.AddSingleton<IReportExportRepository, InMemoryReportExportRepository>();
builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();
builder.Services.AddScoped<IReportFileExportService, ReportFileExportService>();
builder.Services.AddScoped<ReportsViewModel>();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Provider/Auth/Login";
        options.AccessDeniedPath = "/Provider/Auth/Denied";

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClinicianOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Clinician");
    });
});

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IPatientAccessRepository, InMemoryPatientAccessRepository>();
builder.Services.AddScoped<IPatientAuthorizationService, PatientAuthorizationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Auto-authenticate Demo Clinician for seamless local testing
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated != true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "20000000-0000-0000-0000-000000000001"),
            new(ClaimTypes.Role, "Clinician"),
            new(ClaimTypes.Name, "Dr. Sarah Mitchell")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        context.User = new ClaimsPrincipal(identity);
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
