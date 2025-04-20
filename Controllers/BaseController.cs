using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace HealthcareSystem.Controllers
{
    public abstract class BaseController : Controller
    {
        protected bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("UserType") != null;
        }

        protected bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserType") == "Admin";
        }

        protected bool IsHospital()
        {
            var userType = HttpContext.Session.GetString("UserType");
            var hospitalId = HttpContext.Session.GetInt32("HospitalId");
            System.Diagnostics.Debug.WriteLine($"IsHospital check - UserType: {userType}, HospitalId: {hospitalId}");
            return userType == "Hospital" && hospitalId.HasValue;
        }

        protected bool IsPatient()
        {
            return HttpContext.Session.GetString("UserType") == "Patient";
        }

        protected int? GetHospitalId()
        {
            var hospitalId = HttpContext.Session.GetInt32("HospitalId");
            System.Diagnostics.Debug.WriteLine($"GetHospitalId called - HospitalId: {hospitalId}");
            return hospitalId;
        }

        protected int? GetPatientId()
        {
            return HttpContext.Session.GetInt32("PatientId");
        }

        protected string GetHospitalName()
        {
            return HttpContext.Session.GetString("HospitalName");
        }

        protected string GetPatientName()
        {
            return HttpContext.Session.GetString("PatientName");
        }

        protected IActionResult RedirectToLogin()
        {
            System.Diagnostics.Debug.WriteLine("Redirecting to login");
            
            // Get the current controller name
            var controllerName = this.GetType().Name.Replace("Controller", "");
            
            // Redirect to the appropriate login page based on the controller
            switch (controllerName)
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
    }
} 