using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HealthcareSystem.Controllers
{
    public class HospitalController : BaseController
    {
        private readonly HealthcareDbContext _context;
        private readonly ILogger<HospitalController> _logger;

        public HospitalController(HealthcareDbContext context, ILogger<HospitalController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Hospital
        public IActionResult Index()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            var hospital = _context.Hospitals
                .Include(h => h.Doctors)
                .FirstOrDefault(h => h.HospitalId == hospitalId);

            if (hospital == null)
            {
                TempData["Error"] = "Hospital not found.";
                return RedirectToLogin();
            }

            return View(hospital);
        }

        // GET: Hospital/Login
        public IActionResult Login()
        {
            // Clear any existing error messages
            ViewBag.ErrorMessage = null;
            return View();
        }

        // POST: Hospital/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please enter both username and password";
                System.Diagnostics.Debug.WriteLine("Error: Username or password is empty");
                return View();
            }

            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.UserName == userName);

            if (hospital == null)
            {
                ViewBag.ErrorMessage = "Invalid username or password";
                System.Diagnostics.Debug.WriteLine("Error: Hospital not found with username: " + userName);
                return View();
            }

            if (hospital.Password != password)
            {
                ViewBag.ErrorMessage = "Invalid username or password";
                System.Diagnostics.Debug.WriteLine("Error: Password mismatch for hospital: " + hospital.HospitalId);
                return View();
            }

            HttpContext.Session.SetInt32("HospitalId", hospital.HospitalId);
            HttpContext.Session.SetString("HospitalName", hospital.Name);
            HttpContext.Session.SetString("UserType", "Hospital");
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Hospital/Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("HospitalId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // GET: Hospital/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: Hospital/ChangePassword
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("HospitalId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // POST: Hospital/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (HttpContext.Session.GetInt32("HospitalId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

            int hospitalId = HttpContext.Session.GetInt32("HospitalId").Value;
            var hospital = await _context.Hospitals.FindAsync(hospitalId);

            if (hospital == null)
            {
                return NotFound();
            }

            if (hospital.Password != currentPassword)
            {
                ModelState.AddModelError("", "Current password is incorrect");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New passwords do not match");
                return View();
            }

            hospital.Password = newPassword;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Hospital/Doctors
        public async Task<IActionResult> Doctors()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            var doctors = await _context.Doctors
                .Where(d => d.HospitalId == hospitalId)
                .ToListAsync();

            return View(doctors);
        }

        // GET: Hospital/CreateDoctor
        public IActionResult CreateDoctor()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            return View();
        }

        // POST: Hospital/CreateDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(Doctor doctor)
        {
            System.Diagnostics.Debug.WriteLine("CreateDoctor POST action started");
            
            if (!IsHospital())
            {
                System.Diagnostics.Debug.WriteLine("User is not authenticated as hospital");
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            System.Diagnostics.Debug.WriteLine($"HospitalId from session: {hospitalId}");
            
            if (!hospitalId.HasValue)
            {
                System.Diagnostics.Debug.WriteLine("HospitalId is null");
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            // Remove Hospital from ModelState to prevent validation errors
            ModelState.Remove("Hospital");

            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }
                return View(doctor);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Doctor data - Name: {doctor.Name}, Speciality: {doctor.Speciality}, Qualification: {doctor.AcademicQualification}");
                doctor.HospitalId = hospitalId.Value;
                
                _context.Add(doctor);
                System.Diagnostics.Debug.WriteLine("Doctor added to context");
                
                var result = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"SaveChangesAsync result: {result} rows affected");
                
                TempData["Success"] = "Doctor added successfully.";
                return RedirectToAction(nameof(Doctors));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error saving doctor: {ex.Message}");
                return View(doctor);
            }
        }

        // GET: Hospital/EditDoctor/5
        public async Task<IActionResult> EditDoctor(int? id)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id && d.HospitalId == GetHospitalId());
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Hospital/EditDoctor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(int id, Doctor doctor)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            if (id != doctor.DoctorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    doctor.HospitalId = GetHospitalId().Value;
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Doctors));
            }
            return View(doctor);
        }

        // GET: Hospital/DeleteDoctor/5
        public async Task<IActionResult> DeleteDoctor(int? id)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id && d.HospitalId == GetHospitalId());
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Hospital/DeleteDoctor/5
        [HttpPost, ActionName("DeleteDoctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorConfirmed(int id)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id && d.HospitalId == GetHospitalId());
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Doctors));
        }

        // GET: Hospital/Patients
        public async Task<IActionResult> Patients()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            var patients = await _context.Patients
                .Where(p => _context.PatientRecords.Any(pr => pr.HospitalId == hospitalId && pr.PatientId == p.PatientId))
                .ToListAsync();

            return View(patients);
        }

        // GET: Hospital/CreatePatient
        public IActionResult CreatePatient()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // POST: Hospital/CreatePatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient(Patient patient)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Patients));
            }
            return View(patient);
        }

        // GET: Hospital/PatientRecords
        public IActionResult PatientRecords()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // GET: Hospital/PatientSearch
        public IActionResult PatientSearch()
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }
            return View();
        }

        // POST: Hospital/SearchPatient
        [HttpPost]
        public async Task<IActionResult> SearchPatient(string citizenshipNumber)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.CitizenshipNumber == citizenshipNumber);
            
            if (patient == null)
            {
                TempData["Message"] = "Patient not found";
                return RedirectToAction(nameof(PatientRecords));
            }

            var records = await _context.PatientRecords
                .Include(r => r.Doctor)
                .Include(r => r.Hospital)
                .Where(r => r.PatientId == patient.PatientId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View("PatientRecords", records);
        }

        // GET: Hospital/AddAppointment/5
        public async Task<IActionResult> AddAppointment(int patientId)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            var doctors = await _context.Doctors
                .Where(d => d.HospitalId == hospitalId)
                .ToListAsync();

            ViewBag.PatientId = patientId;
            ViewBag.Doctors = doctors;
            return View(new Appointment { PatientId = patientId, HospitalId = hospitalId.Value, Status = "Pending" });
        }

        // POST: Hospital/AddAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAppointment(Appointment appointment)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                var doctors = await _context.Doctors
                    .Where(d => d.HospitalId == hospitalId)
                    .ToListAsync();
                ViewBag.Doctors = doctors;
                ViewBag.PatientId = appointment.PatientId;
                return View(appointment);
            }

            try
            {
                appointment.HospitalId = hospitalId.Value;
                appointment.Status = "Pending";
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment scheduled successfully.";
                return RedirectToAction(nameof(PatientRecords));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error scheduling appointment: {ex.Message}");
                var doctors = await _context.Doctors
                    .Where(d => d.HospitalId == hospitalId)
                    .ToListAsync();
                ViewBag.Doctors = doctors;
                ViewBag.PatientId = appointment.PatientId;
                return View(appointment);
            }
        }

        // GET: Hospital/AddPatientRecord/5
        public async Task<IActionResult> AddPatientRecord(int patientId)
        {
            if (!IsHospital())
            {
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            var doctors = await _context.Doctors
                .Where(d => d.HospitalId == hospitalId)
                .ToListAsync();

            ViewBag.Doctors = doctors;
            return View(new PatientRecord 
            { 
                PatientId = patientId,
                HospitalId = hospitalId.Value,
                RecordDate = DateTime.Now
            });
        }

        // POST: Hospital/AddPatientRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatientRecord(PatientRecord record)
        {
            _logger.LogInformation("AddPatientRecord action started");
            
            if (!IsHospital())
            {
                _logger.LogWarning("User is not authenticated as hospital");
                return RedirectToLogin();
            }

            var hospitalId = GetHospitalId();
            if (!hospitalId.HasValue)
            {
                _logger.LogWarning("Hospital ID not found in session");
                TempData["Error"] = "Hospital ID not found. Please log in again.";
                return RedirectToLogin();
            }

            // Remove navigation properties from ModelState to prevent validation errors
            ModelState.Remove("Doctor");
            ModelState.Remove("Patient");
            ModelState.Remove("Hospital");

            _logger.LogInformation($"Model State Valid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning($"Validation Error: {error.ErrorMessage}");
                    }
                }

                var doctors = await _context.Doctors
                    .Where(d => d.HospitalId == hospitalId)
                    .ToListAsync();
                ViewBag.Doctors = doctors;
                return View(record);
            }

            try
            {
                _logger.LogInformation($"Attempting to save record - PatientId: {record.PatientId}, DoctorId: {record.DoctorId}");
                record.HospitalId = hospitalId.Value;
                record.RecordDate = DateTime.Now;

                _logger.LogInformation("Record details before save:");
                _logger.LogInformation($"- PatientId: {record.PatientId}");
                _logger.LogInformation($"- DoctorId: {record.DoctorId}");
                _logger.LogInformation($"- HospitalId: {record.HospitalId}");
                _logger.LogInformation($"- Diagnosis: {record.Diagnosis}");
                _logger.LogInformation($"- Prescription: {record.Prescription}");
                _logger.LogInformation($"- Notes: {record.Notes}");
                _logger.LogInformation($"- RecordDate: {record.RecordDate}");

                _context.Add(record);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Record saved successfully with ID: {record.RecordId}");
                TempData["Success"] = "Patient record added successfully.";
                return RedirectToAction(nameof(PatientRecords));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving patient record");
                ModelState.AddModelError("", $"Error adding patient record: {ex.Message}");
                var doctors = await _context.Doctors
                    .Where(d => d.HospitalId == hospitalId)
                    .ToListAsync();
                ViewBag.Doctors = doctors;
                return View(record);
            }
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
} 