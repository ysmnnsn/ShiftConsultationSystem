using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShiftConsultationSystem.Models;

public class DoctorController : Controller
{
    private readonly AppDbContext _context;

    public DoctorController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult DoctorHomepage()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            // If session is null, redirect to the login page
            return RedirectToAction("Login", "Account");
        }

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            ViewBag.UserName = user.Name;
            return View();
        }

        // If no user is found, redirect to login
        return RedirectToAction("Login", "Account");
    }




    public IActionResult ViewShifts()
    {
        // Fetch all shifts from the database
        var shifts = _context.Shifts
                             .Include(s => s.User)  // Assuming Shift has a User relationship
                             .OrderBy(s => s.ShiftDate)  // Order shifts by date
                             .ToList();

        return View(shifts);  // Pass the list of shifts to the view
    }



    // GET: Display the form to request a consultation
    [HttpGet]
    public IActionResult RequestConsultation()
    {
        // Fetch the list of departments and hospitals from the database
        var departments = _context.Departments.ToList();
        var hospitals = _context.Hospitals.ToList();

        // Convert departments and hospitals to SelectListItem
        ViewBag.Departments = departments.Select(d => new SelectListItem
        {
            Value = d.DepartmentId.ToString(),  // Use DepartmentId as the value
            Text = d.DepartmentName             // Use DepartmentName as the display text
        }).ToList();

        ViewBag.Hospitals = hospitals.Select(h => new SelectListItem
        {
            Value = h.HospitalId.ToString(),  // Use HospitalId as the value
            Text = h.HospitalName             // Use HospitalName as the display text
        }).ToList();

        return View();
    }

    // POST: Handle the submission of the consultation request form
    [HttpPost]
    public IActionResult RequestConsultation(string patientName, string roomNumber, int departmentId, int hospitalId, string consultationNote)
    {
        // Get the current logged-in user’s ID (who is making the request)
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return View("Error", new ErrorViewModel { Message = "Unable to retrieve user information." });
        }

        // Parse the user ID from the claim
        int requesterId = int.Parse(userIdClaim);

        // Create a new consultation request object
        var consultationRequest = new ConsultationRequest
        {
            RequesterId = requesterId,  // The user who is making the request
            PatientName = patientName,  // Patient's name
            RoomNumber = roomNumber,    // Room number
            DepartmentId = departmentId,  // The department being requested for consultation
            HospitalId = hospitalId,    // The hospital being requested for consultation
            ConsultationNote = consultationNote,  // Note on why the consultation is needed
            IsHandled = false  // New requests are unhandled by default
        };

        // Add the new consultation request to the database
        _context.ConsultationRequests.Add(consultationRequest);
        _context.SaveChanges();  // Save changes to the database

        // Redirect to a confirmation page or homepage
        return RedirectToAction("DoctorHomepage");
    }


    public IActionResult ViewPendingConsultations()
    {
        // Check if the user is authenticated
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");  // Redirect to login if not authenticated
        }

        // Get the logged-in user's ID
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return View("Error", new ErrorViewModel { Message = "Unable to retrieve user information." });
        }

        // Convert the user ID to an integer
        int currentUserId = int.Parse(userIdClaim);

        // Fetch the current user's details, including their department and hospital
        var currentUser = _context.Users
                                  .Include(u => u.Department)  // Include the department relationship
                                  .Include(u => u.Hospital)    // Include the hospital relationship
                                  .FirstOrDefault(u => u.UserId == currentUserId);

        // Check if the user is associated with both a department and hospital
        if (currentUser == null || currentUser.Department == null || currentUser.Hospital == null)
        {
            return View("Error", new ErrorViewModel { Message = "You are not associated with any department or hospital." });
        }

        // Retrieve the current user's hospital ID and department ID
        int currentUserHospitalId = currentUser.HospitalId;
        int currentUserDepartmentId = currentUser.DepartmentId;

        // Filter consultation requests by both hospital ID and department ID (matching the current user's details)
        var pendingRequests = _context.ConsultationRequests
                                      .Where(c => !c.IsHandled && c.HospitalId == currentUserHospitalId && c.DepartmentId == currentUserDepartmentId)  // Filter by hospital and department
                                      .Include(c => c.Requester)
                                      .Include(c => c.Department)
                                      .Include(c => c.Hospital)  // Include hospital data for display
                                      .ToList();

        // Return the filtered consultation requests to the view
        return View("~/Views/Shared/ViewPendingConsultations.cshtml", pendingRequests);
    }





    public IActionResult ViewConsultationHistory()
    {
        // Get the logged-in user's ID from the session or claims
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return RedirectToAction("Login", "Account"); // Redirect to login if user is not authenticated
        }

        var currentUserId = int.Parse(userIdClaim);

        // Get consultation requests made by the current user and who accepted them
        var userRequests = _context.ConsultationRequests
                                   .Include(c => c.Acceptances)
                                   .ThenInclude(a => a.Doctor) // Include the doctor who accepted the consultation
                                   .Where(c => c.RequesterId == currentUserId) // Filter by current user's requests
                                   .ToList();

        // Get consultation requests that the current user has accepted
        var acceptedRequests = _context.ConsultationAcceptances
                                       .Include(a => a.ConsultationRequest)
                                       .ThenInclude(c => c.Requester) // Include the requester details
                                       .Where(a => a.DoctorId == currentUserId) // Filter by current user's acceptances
                                       .ToList();

        // Create a model to pass both lists to the view
        var model = new ConsultationHistoryViewModel
        {
            UserRequests = userRequests,
            AcceptedRequests = acceptedRequests
        };

        return View(model); // Pass the model to the view
    }




    [HttpPost]
    public IActionResult AcceptConsultation(int consultationRequestId)
    {
        // Get the consultation request by ID
        var consultationRequest = _context.ConsultationRequests
                                          .Include(c => c.Requester)
                                          .FirstOrDefault(c => c.ConsultationRequestId == consultationRequestId);

        if (consultationRequest != null && !consultationRequest.IsHandled)
        {
            // Mark the consultation request as handled
            consultationRequest.IsHandled = true;
            _context.SaveChanges();

            // Get the logged-in user's ID
            var currentDoctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Create a new ConsultationAcceptance record
            var consultationAcceptance = new ConsultationAcceptance
            {
                ConsultationRequestId = consultationRequest.ConsultationRequestId,
                DoctorId = currentDoctorId,  // The doctor accepting the request
                AcceptanceDate = DateTime.Now
            };

            // Save the acceptance
            _context.ConsultationAcceptances.Add(consultationAcceptance);
            _context.SaveChanges();
        }

        // Redirect back to the pending consultations view
        return RedirectToAction("ViewPendingConsultations");
    }

}


