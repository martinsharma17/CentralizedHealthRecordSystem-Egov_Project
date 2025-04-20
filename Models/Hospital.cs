using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthcareSystem.Models
{
    public class Hospital
    {
        public Hospital()
        {
            Doctors = new HashSet<Doctor>();
        }

        [Key]
        public int HospitalId { get; set; }

        [Required]
        [Display(Name = "Hospital Name")]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [RegularExpression(@"^9\d{9}$", ErrorMessage = "Phone number must start with 9 and be exactly 10 digits.")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        // Navigation properties
        public virtual ICollection<Doctor> Doctors { get; set; }
    }
} 