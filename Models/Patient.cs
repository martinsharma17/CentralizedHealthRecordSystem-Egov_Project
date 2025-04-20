using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthcareSystem.Models
{
    public class Patient
    {
        public Patient()
        {
            Appointments = new HashSet<Appointment>();
            PatientRecords = new HashSet<PatientRecord>();
        }

        [Key]
        public int PatientId { get; set; }

        [Required]
        [Display(Name = "Patient Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Citizenship Number")]
        public string CitizenshipNumber { get; set; }

        [Required]
        public string Sex { get; set; }

        [Required]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; }

        [Required]
        [RegularExpression(@"^9\d{9}$", ErrorMessage = "Phone number must start with 9 and be exactly 10 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Password { get; set; }

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<PatientRecord> PatientRecords { get; set; }
    }
} 