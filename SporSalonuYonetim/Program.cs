using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using System.Text.Json.Serialization; // JSON Döngüsü için gerekli kütüphane
using StackExchange.Profiling;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. IDENTITY Ayarları
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Şifre Kuralları
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;

    // Email ve Giriş Kuralları
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;

    // Türkçe karakter ve boşluk izni
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Yönlendirme Ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 4. Sahte Email Servisi
builder.Services.AddSingleton<IEmailSender, EmailSender>();

// 5. JSON, SWAGGER VE MINIPROFILER AYARLARI
builder.Services.AddControllersWithViews()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddRazorPages();

// Swagger Servisleri
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔥 MINIPROFILER SERVİSİ (YENİ EKLENDİ) 🔥
// Bu kısım SQL sorgularını takip etmeni sağlar
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
}).AddEntityFramework();

var app = builder.Build();

// 6. Otomatik Admin Oluşturma (Seeding)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Uye"))
            await roleManager.CreateAsync(new IdentityRole("Uye"));

        var adminEmail = "ogrencinumarasi@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "sau");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Admin oluşturulamadı.");
    }
}

// 7. Pipeline Ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Swagger Aktif
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 🔥 MINIPROFILER MİDDLEWARE (YENİ EKLENDİ) 🔥
app.UseMiniProfiler();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// ---------------------------------------------------------
// Sahte Email Sınıfı
public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}
// ---------------------------------------------------------