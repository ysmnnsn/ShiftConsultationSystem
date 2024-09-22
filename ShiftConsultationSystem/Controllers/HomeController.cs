using Microsoft.AspNetCore.Mvc;
using ShiftConsultationSystem.Models;
using System.Diagnostics;
using System.Security.Claims;   // For cookie claims handling


namespace ShiftConsultationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Retrieve the UserId and UserRole from the session
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Pass the data to the view using ViewBag or ViewData
            ViewBag.UserId = userId;
            ViewBag.UserRole = userRole;

            // Get the user's role from the session or claims
        
        if (string.IsNullOrEmpty(userRole))
        {
            // If session is empty, try getting the role from the claims (cookie-based authentication)
            userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        }

            // Check if the user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Redirect to the correct dashboard based on role
                if (role == "admin")
                    return RedirectToAction("AdminHomepage", "Admin");
                else if (role == "doctor")
                    return RedirectToAction("DoctorHomepage", "Doctor");
            }

            // If the user is not authenticated, redirect to the login page
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
