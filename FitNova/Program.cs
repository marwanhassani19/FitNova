using FitNova.Data;
using FitNova.Models;
using FitNova.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=fitnova.db"));

// 2. IDENTITY
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// 3. SERVIZI DI SISTEMA
builder.Services.AddRazorPages();
// 1. Registra HttpClient
builder.Services.AddHttpClient();

// 2. Leggi la chiave API
var geminiApiKey = builder.Configuration["GeminiSettings:ApiKey"];

// 3. Registra il servizio (UNA SOLA VOLTA)
builder.Services.AddScoped<FitNova.Services.GeminiService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new FitNova.Services.GeminiService(httpClient, geminiApiKey ?? "");
});
var app = builder.Build();

// 5. MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// 6. SEED DATABASE (ADMIN + RUOLI)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));

    var adminEmail = "admin@fitnova.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Admin" };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.Run();