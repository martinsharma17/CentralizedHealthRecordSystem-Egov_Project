using Microsoft.EntityFrameworkCore;
using HealthcareSystem.Models;
using Microsoft.Extensions.Logging;

namespace HealthcareSystem.Data
{
    public class HealthcareDbContext : DbContext
    {
        private readonly ILogger<HealthcareDbContext> _logger;

        public HealthcareDbContext(DbContextOptions<HealthcareDbContext> options, ILogger<HealthcareDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientRecord> PatientRecords { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints
            modelBuilder.Entity<Hospital>()
                .HasIndex(h => h.UserName)
                .IsUnique();

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.CitizenshipNumber)
                .IsUnique();

            // Configure Hospital-Doctor relationship
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Hospital)
                .WithMany(h => h.Doctors)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Patient-Appointments relationship
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Hospital)
                .WithMany()
                .HasForeignKey(a => a.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Patient-PatientRecords relationship
            modelBuilder.Entity<PatientRecord>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.PatientRecords)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientRecord>()
                .HasOne(r => r.Doctor)
                .WithMany()
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientRecord>()
                .HasOne(r => r.Hospital)
                .WithMany()
                .HasForeignKey(r => r.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Notes property
            modelBuilder.Entity<PatientRecord>()
                .Property(r => r.Notes)
                .IsRequired(false);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to save changes to the database");
                var result = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Successfully saved {result} changes to the database");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving changes to the database");
                throw;
            }
        }
    }
} 