using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HealthcareSystem.Controllers
{
    public class PatientController : BaseController
    {
        private readonly HealthcareDbContext _context;

        public PatientController(HealthcareDbContext context)
        {
            _context = context;
        }

        // GET: Patient
        public IActionResult Index()
        {
            if (!IsPatient())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // GET: Patient/Records
        public async Task<IActionResult> Records()
        {
            if (!IsPatient())
            {
                return RedirectToLogin();
            }

            var patientId = GetPatientId();
            var patient = await _context.Patients.FindAsync(patientId);
            
            if (patient == null)
            {
                return NotFound();
            }

            var records = await _context.PatientRecords
                .Include(r => r.Doctor)
                .Include(r => r.Hospital)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View(records);
        }

        // GET: Patient/Appointments
        public async Task<IActionResult> Appointments()
        {
            if (!IsPatient())
            {
                return RedirectToLogin();
            }

            var patientId = GetPatientId();
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Patient/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Patient/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string citizenshipNumber, string password)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.CitizenshipNumber == citizenshipNumber && p.Password == password);

            if (patient != null)
            {
                HttpContext.Session.SetInt32("PatientId", patient.PatientId);
                HttpContext.Session.SetString("PatientName", patient.Name);
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        // GET: Patient/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetInt32("PatientId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

            int patientId = HttpContext.Session.GetInt32("PatientId").Value;
            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                return NotFound();
            }

            // Get patient records
            var patientRecords = await _context.PatientRecords
                .Include(pr => pr.Doctor)
                .Include(pr => pr.Hospital)
                .Where(pr => pr.PatientId == patientId)
                .OrderByDescending(pr => pr.RecordDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.PatientRecords = patientRecords;

            return View();
        }

        // GET: Patient/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: Patient/ChangePassword
        public IActionResult ChangePassword()
        {
            if (!IsPatient())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // POST: Patient/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!IsPatient())
            {
                return RedirectToLogin();
            }

            var patientId = GetPatientId();
            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                return NotFound();
            }

            if (patient.Password != currentPassword)
            {
                ModelState.AddModelError("", "Current password is incorrect");
                return View();
            }

            patient.Password = newPassword;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: Patient/CreateAppointment
        public async Task<IActionResult> CreateAppointment()
        {
            ViewBag.Hospitals = new SelectList(await _context.Hospitals.ToListAsync(), "HospitalId", "Name");
            ViewBag.Doctors = new SelectList(Enumerable.Empty<SelectListItem>());
            return View();
        }

        // POST: Patient/CreateAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment([Bind("HospitalId,DoctorId,AppointmentDate")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                var patientId = HttpContext.Session.GetInt32("PatientId");
                if (patientId == null)
                {
                    return RedirectToAction("Login");
                }

                appointment.PatientId = patientId.Value;
                appointment.Status = "Pending";
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Appointments));
            }

            ViewBag.Hospitals = new SelectList(await _context.Hospitals.ToListAsync(), "HospitalId", "Name");
            ViewBag.Doctors = new SelectList(await _context.Doctors
                .Where(d => d.HospitalId == appointment.HospitalId)
                .ToListAsync(), "DoctorId", "Name");
            return View(appointment);
        }

        // GET: Patient/GetDoctors
        public async Task<JsonResult> GetDoctors(int hospitalId)
        {
            var doctors = await _context.Doctors
                .Where(d => d.HospitalId == hospitalId)
                .Select(d => new { value = d.DoctorId, text = d.Name })
                .ToListAsync();
            return Json(doctors);
        }
    }
} 