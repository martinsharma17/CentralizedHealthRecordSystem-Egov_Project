using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthcareSystem.Models
{
    [Index(nameof(PatientId))]
    [Index(nameof(DoctorId))]
    [Index(nameof(HospitalId))]
    public class PatientRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int HospitalId { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        [Required]
        public string Prescription { get; set; }

        public string Notes { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        [ForeignKey("HospitalId")]
        public virtual Hospital Hospital { get; set; }
    }
} 