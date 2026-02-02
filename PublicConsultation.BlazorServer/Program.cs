using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PublicConsultation.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; })
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    });
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

// Custom Services
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IAuthService, PublicConsultation.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IDocumentService, PublicConsultation.Infrastructure.Services.DocumentService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IEmailService, PublicConsultation.Infrastructure.Services.EmailService>();
// Configure HttpClient for Python AI Service
var aiServiceUrl = builder.Configuration.GetValue<string>("AiServiceUrl") ?? "http://localhost:5000/";
builder.Services.AddHttpClient<PublicConsultation.Core.Interfaces.IAiAnalysisService, PublicConsultation.Infrastructure.Services.AiAnalysisService>(client =>
{
    client.BaseAddress = new Uri(aiServiceUrl);
});
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IDuplicateDetectionService, PublicConsultation.Infrastructure.Services.DuplicateDetectionService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IAuditLogService, PublicConsultation.Infrastructure.Services.AuditLogService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IMinistryService, PublicConsultation.Infrastructure.Services.MinistryService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IFileService, PublicConsultation.Infrastructure.Services.FileService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.IBiometricService, PublicConsultation.Infrastructure.Services.BiometricService>();
builder.Services.AddScoped<PublicConsultation.Core.Interfaces.ILocationService, PublicConsultation.Infrastructure.Services.LocationService>();
builder.Services.AddHostedService<PublicConsultation.Infrastructure.Services.ConsultationStatusHostedService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });
    // The default HSTS value is 30 days...
    app.UseHsts();
}
else
{
    // Also use for development if testing behind a proxy
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var authService = services.GetRequiredService<PublicConsultation.Core.Interfaces.IAuthService>();
        context.Database.EnsureCreated();
        await DbSeeder.SeedAsync(context, authService);
        Log.Information("Database created and seeded successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred creating the DB.");
    }
}

app.Run();
