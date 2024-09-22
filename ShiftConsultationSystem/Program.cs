using Microsoft.EntityFrameworkCore;
using ShiftConsultationSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies; // Add this for cookie authentication
using Hangfire;  

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Hangfire
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session handling
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout
    options.Cookie.HttpOnly = true;  // Only accessible via HTTP
    options.Cookie.IsEssential = true;  // Mark session as essential
});

// Configure authentication with cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";  // Redirect to login if not authenticated
        options.AccessDeniedPath = "/Account/AccessDenied";  // Redirect to access denied page
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Set cookie expiration time
    });

// Add authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session
app.UseSession();  // This enables session state for your application

// Enable authentication and authorization middleware
app.UseAuthentication();  // Make sure this is added before UseAuthorization
app.UseAuthorization();

app.UseHangfireDashboard(); // Optional: This provides a dashboard to manage jobs

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
