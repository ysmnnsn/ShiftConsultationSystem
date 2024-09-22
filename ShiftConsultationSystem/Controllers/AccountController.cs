using Microsoft.AspNetCore.Mvc;
using ShiftConsultationSystem.Models;  // Import your models
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Authenticate the user from the database
        var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

        if (user != null)
        {
            // Sign in the user with cookie authentication
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)  // Assign the user's role as a claim
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Whether the authentication session is persistent
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30)  // Cookie expiration time
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(claimsIdentity),
                                          authProperties);

            // Set user information in session (optional, as the cookie is already handling roles)
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "admin")
                return RedirectToAction("AdminHomepage", "Admin");
            else
                return RedirectToAction("DoctorHomepage", "Doctor");
        }

        ModelState.AddModelError("", "Invalid username or password.");
        return View();
    }




    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Sign the user out by clearing the authentication cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
