using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShiftConsultationSystem.Models;
using Hangfire;


namespace ShiftConsultationSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
            RecurringJob.AddOrUpdate("distribute-shifts", () => CheckAndDistributeShifts(), Cron.Monthly(1));
        }

        public void CheckAndDistributeShifts()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            bool shiftsExist = _context.Shifts.Any(s => s.ShiftDate.Month == currentMonth && s.ShiftDate.Year == currentYear);

            if (!shiftsExist)
            {
                DistributeShifts();
            }
        }

        // Admin Homepage
        public IActionResult AdminHomepage()
        {
            // Assuming the user is logged in and their ID is stored in the session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    ViewBag.UserName = user.Name;  // Pass the user's name to the view
                }
            }

            return View();
        }

        // Method to distribute shifts evenly
        public void DistributeShifts()
        {
            // Get the current month and number of days in the month
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            // Get all users from the database
            var users = _context.Users.ToList();
            int numberOfUsers = users.Count;

            if (numberOfUsers == 0) return;  // No users to assign shifts

            // Remove any existing shifts for the current month before redistributing
            var existingShifts = _context.Shifts.Where(s => s.ShiftDate.Month == currentMonth && s.ShiftDate.Year == currentYear);
            _context.Shifts.RemoveRange(existingShifts);

            // Loop through each day of the current month and assign a user to that day
            for (int day = 1; day <= daysInMonth; day++)
            {
                // Assign the shift to a user, using modulus to loop through users
                var assignedUser = users[(day - 1) % numberOfUsers];

                // Create a new shift entry
                var shift = new Shift
                {
                    ShiftDate = new DateTime(currentYear, currentMonth, day),
                    UserId = assignedUser.UserId  // Assign the user for this day
                };

                // Add the shift to the database
                _context.Shifts.Add(shift);
            }

            // Save the changes to the database
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult RedistributeShifts()
        {
            DistributeShifts();
            return RedirectToAction("ViewShifts");
        }

        // Add User (Admin action)
        public IActionResult AddUser()
        {
            // Fetch the list of departments and hospitals from the database
            var departments = _context.Departments.ToList();
            var hospitals = _context.Hospitals.ToList();

            // Convert departments and hospitals to SelectListItem
            ViewBag.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),  // Use the ID as the value
                Text = d.DepartmentName             // Use the name as the displayed text
            }).ToList();

            ViewBag.Hospitals = hospitals.Select(h => new SelectListItem
            {
                Value = h.HospitalId.ToString(),
                Text = h.HospitalName
            }).ToList();

            return View();
        }

        public IActionResult ViewUsers()
        {
            var users = _context.Users.ToList();  // Fetch all users from the database
            return View(users);  // Pass the list of users to the view
        }

        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);  // Return a view to confirm deletion
        }

        // POST: Admin/DeleteUserConfirmed
        [HttpPost]
        public IActionResult DeleteUserConfirmed(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                _context.Users.Remove(user);  // Remove the user from the database
                DistributeShifts();
                _context.SaveChanges();  // Save changes to the database
            }

            
            return RedirectToAction("ViewUsers");  // Redirect to user list after deletion
        }

        [HttpPost]
        public IActionResult AddUser(string name, string username, string password, int departmentId, int hospitalId)
        {
            var user = new User
            {
                Name = name,  // Ensure the Name is being set
                Username = username,
                Password = password,
                Role = "doctor",  // You can change the default role as needed
                DepartmentId = departmentId,
                HospitalId = hospitalId
            };

            _context.Users.Add(user);
            _context.SaveChanges();  // Save the new user to the database

            DistributeShifts();
            return RedirectToAction("AdminHomepage");
        }


        // View all shifts (Admin action)
        public IActionResult ViewShifts()
        {
            // Fetch all shifts from the database
            var shifts = _context.Shifts
                                 .Include(s => s.User)  // Assuming Shift has a User relationship
                                 .OrderBy(s => s.ShiftDate)  // Order shifts by date
                                 .ToList();

            return View(shifts);  // Pass the list of shifts to the view
        }



        // Edit shifts (Admin action)
        public IActionResult EditShifts()
        {
            // Fetch all shifts, including the associated user (doctor)
            var shifts = _context.Shifts.Include(s => s.User).ToList();

            // Fetch both doctors and admins to display in the dropdown
            var doctorsAndAdmins = _context.Users
                                           .Where(u => u.Role == "doctor" || u.Role == "admin")
                                           .ToList();

            // Pass the combined list of doctors and admins to the view
            ViewBag.DoctorsAndAdmins = doctorsAndAdmins;

            return View(shifts);
        }


        [HttpPost]
        public IActionResult UpdateShift(int shiftId, int newDoctorId)
        {
            var shift = _context.Shifts.FirstOrDefault(s => s.ShiftId == shiftId);

            if (shift != null)
            {
                shift.UserId = newDoctorId;
                _context.SaveChanges();
            }

            return RedirectToAction("EditShifts");
        }

        // Manage Departments (Admin action)
        public IActionResult ManageDepartments()
        {
            var departments = _context.Departments.ToList();
            return View(departments);
        }

        [HttpPost]
        public IActionResult AddDepartment(string departmentName)
        {
            var department = new Department { DepartmentName = departmentName };
            _context.Departments.Add(department);
            _context.SaveChanges();
            return RedirectToAction("ManageDepartments");
        }

        // Manage Hospitals (Admin action)
        public IActionResult ManageHospitals()
        {
            var hospitals = _context.Hospitals.ToList();
            return View(hospitals);
        }

        [HttpPost]
        public IActionResult AddHospital(string hospitalName)
        {
            var hospital = new Hospital { HospitalName = hospitalName };
            _context.Hospitals.Add(hospital);
            _context.SaveChanges();
            return RedirectToAction("ManageHospitals");
        }

        public IActionResult ViewPendingConsultations()
        {
            // Check if the user is authenticated using cookies
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");  // Redirect to login if not authenticated
            }

            // Retrieve the user's role from either session or claims
            var userRole = HttpContext.Session.GetString("UserRole");  // Check session first
            if (string.IsNullOrEmpty(userRole))
            {
                // If the session doesn't have the role, try retrieving it from the claims
                userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            }

            // Ensure that the role is set and user is allowed to view consultations
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the logged-in user's ID from claims (set during login)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return View("Error", new ErrorViewModel { Message = "Unable to retrieve user information." });
            }

            // Convert the user ID to an integer
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return View("Error", new ErrorViewModel { Message = "Invalid user ID format." });
            }

            // Fetch the current user's details, including their department and hospital
            var currentUser = _context.Users
                                      .Include(u => u.Department)
                                      .Include(u => u.Hospital)
                                      .FirstOrDefault(u => u.UserId == currentUserId);

            if (currentUser == null || currentUser.Department == null || currentUser.Hospital == null)
            {
                return View("Error", new ErrorViewModel { Message = "You are not associated with any department or hospital." });
            }

            // Retrieve the current user's hospital and department ID
            int currentUserHospitalId = currentUser.HospitalId;
            int currentUserDepartmentId = currentUser.DepartmentId;

            // Filter consultation requests by hospital and department
            var pendingRequests = _context.ConsultationRequests
                                          .Where(c => !c.IsHandled && c.HospitalId == currentUserHospitalId && c.DepartmentId == currentUserDepartmentId)
                                          .Include(c => c.Requester)
                                          .Include(c => c.Department)
                                          .Include(c => c.Hospital)
                                          .ToList();

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
            return RedirectToAction("AdminHomepage");
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
}
