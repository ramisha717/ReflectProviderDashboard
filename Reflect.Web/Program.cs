using Microsoft.AspNetCore.Authentication.Cookies;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.Reports;
using Reflect.Web.Repositories;
using Reflect.Web.Services;
using Reflect.Web.Standins;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddSingleton<IPatientRepository, InMemoryPatientRepository>();
builder.Services.AddSingleton<IPatientSessionRepository, InMemoryPatientSessionRepository>();
builder.Services.AddSingleton<IClinicalObservationRepository, InMemoryClinicalObservationRepository>();
builder.Services.AddSingleton<IMonitoringDraftRepository, InMemoryMonitoringDraftRepository>();
builder.Services.AddSingleton<ISummaryGenerationService, TemplateSummaryGenerationService>();
builder.Services.AddSingleton<IReportExportRepository, InMemoryReportExportRepository>();
builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();
builder.Services.AddScoped<IReportFileExportService, ReportFileExportService>();
builder.Services.AddSingleton<IReportSigningService, ReportSigningService>();
builder.Services.AddScoped<ISignedReportService, SignedReportService>();
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
builder.Services.AddSingleton<AttentionAcknowledgementService>();

var app = builder.Build();

// Content Security Policy required by the Reflect XR technical specification.
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' wss:; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "form-action 'self';";

    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseDemoClinicianAuthentication();

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