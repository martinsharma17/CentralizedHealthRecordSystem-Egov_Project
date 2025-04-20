using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareSystem.Data;
using HealthcareSystem.Models;

namespace HealthcareSystem.Controllers
{
    public class AdminController : BaseController
    {
        private readonly HealthcareDbContext _context;

        public AdminController(HealthcareDbContext context)
        {
            _context = context;
        }

        // GET: Admin
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            // If already logged in as admin, redirect to admin dashboard
            if (IsAdmin())
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please enter both username and password";
                return View();
            }

            // Check admin credentials (in a real app, this would be in a database)
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("UserType", "Admin");
                HttpContext.Session.SetString("Username", username);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = "Invalid username or password";
            return View();
        }

        // GET: Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: Admin/Hospitals
        public async Task<IActionResult> Hospitals()
        {
            if (!IsAdmin())
            {
                return RedirectToLogin();
            }

            var hospitals = await _context.Hospitals.ToListAsync();
            return View(hospitals);
        }

        // GET: Admin/CreateHospital
        public IActionResult CreateHospital()
        {
            if (!IsAdmin())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // POST: Admin/CreateHospital
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHospital(Hospital hospital)
        {
            if (!IsAdmin())
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        TempData["Error"] = error.ErrorMessage;
                    }
                }
                return View(hospital);
            }

            try
            {
                _context.Add(hospital);
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    TempData["Message"] = "Hospital created successfully";
                    return RedirectToAction(nameof(Hospitals));
                }
                else
                {
                    TempData["Error"] = "Failed to save hospital data";
                    return View(hospital);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating hospital: {ex.Message}";
                return View(hospital);
            }
        }

        // GET: Admin/DeleteHospital/5
        public async Task<IActionResult> DeleteHospital(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToLogin();
            }

            if (id == null)
            {
                return NotFound();
            }

            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(m => m.HospitalId == id);
            if (hospital == null)
            {
                return NotFound();
            }

            return View(hospital);
        }

        // POST: Admin/DeleteHospital/5
        [HttpPost, ActionName("DeleteHospital")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHospitalConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToLogin();
            }

            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital != null)
            {
                _context.Hospitals.Remove(hospital);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Hospitals));
        }
    }
} 