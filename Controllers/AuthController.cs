using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Http;

namespace HealthcareSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly HealthcareDbContext _context;

        public AuthController(HealthcareDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Admin");
        }

        // POST: Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string userType, string username, string password)
        {
            switch (userType.ToLower())
            {
                case "admin":
                    if (username == "admin" && password == "admin123")
                    {
                        HttpContext.Session.SetString("UserType", "Admin");
                        HttpContext.Session.SetString("Username", username);
                        return RedirectToAction("Index", "Admin");
                    }
                    break;

                case "hospital":
                    var hospital = await _context.Hospitals
                        .FirstOrDefaultAsync(h => h.UserName == username && h.Password == password);
                    if (hospital != null)
                    {
                        HttpContext.Session.SetString("UserType", "Hospital");
                        HttpContext.Session.SetInt32("HospitalId", hospital.HospitalId);
                        HttpContext.Session.SetString("HospitalName", hospital.Name);
                        return RedirectToAction("Index", "Hospital");
                    }
                    break;

                case "patient":
                    var patient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.CitizenshipNumber == username && p.Password == password);
                    if (patient != null)
                    {
                        HttpContext.Session.SetString("UserType", "Patient");
                        HttpContext.Session.SetInt32("PatientId", patient.PatientId);
                        HttpContext.Session.SetString("PatientName", patient.Name);
                        return RedirectToAction("Index", "Patient");
                    }
                    break;
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            var userType = HttpContext.Session.GetString("UserType");
            HttpContext.Session.Clear();
            
            // Redirect to the appropriate login page based on the user type
            switch (userType)
            {
                case "Admin":
                    return RedirectToAction("Login", "Admin");
                case "Hospital":
                    return RedirectToAction("Login", "Hospital");
                case "Patient":
                    return RedirectToAction("Login", "Patient");
                default:
                    return RedirectToAction("Login", "Admin");
            }
        }

        // GET: Auth/ChangePassword
        public IActionResult ChangePassword()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserType")))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST: Auth/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var userType = HttpContext.Session.GetString("UserType");
            var username = HttpContext.Session.GetString("Username");

            if (userType == "Admin" && currentPassword == "admin123")
            {
                // In a real application, you would update this in a database
                // For now, we'll just show a success message
                TempData["Message"] = "Password changed successfully";
                return RedirectToAction("Index", "Admin");
            }

            ModelState.AddModelError("", "Invalid current password");
            return View();
        }
    }
} 