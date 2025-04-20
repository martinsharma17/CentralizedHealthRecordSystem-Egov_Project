using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareSystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Doctor Name")]
        public string Name { get; set; }

        [Required]
        public string Speciality { get; set; }

        [Required]
        [Display(Name = "Academic Qualification")]
        public string AcademicQualification { get; set; }

        // HospitalId is set in the controller, not required in the form
        public int HospitalId { get; set; }

        [ForeignKey("HospitalId")]
        public virtual Hospital? Hospital { get; set; }
    }
} 