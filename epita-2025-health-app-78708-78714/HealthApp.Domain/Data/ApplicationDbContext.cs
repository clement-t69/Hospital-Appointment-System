using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.Domain.Data
{
    public class ApplicationDbContext: IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //public DbSet<User> Users { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Administrator
            modelBuilder.Entity<Administrator>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Administrator>(a => a.UserId);

            // Doctor
            modelBuilder.Entity<Doctor>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Doctor>(d => d.UserId);

            modelBuilder.Entity<Doctor>() // Multiple Doctor Availabilities
                .HasMany(d => d.DoctorAvailabilities)
                .WithOne(da => da.Doctor)
                .HasForeignKey(da => da.DoctorId);

            modelBuilder.Entity<Doctor>() // Multiple Notifications
                .HasMany(d => d.Notifications)
                .WithOne(n => n.Doctor)
                .HasForeignKey(n => n.DoctorId);

            // DoctorAvailability
            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(da => da.Doctor)
                .WithMany(d => d.DoctorAvailabilities)
                .HasForeignKey(da => da.DoctorId);

            // DoctorSpecialization
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Specialization)
                .WithOne()
                .HasForeignKey<Doctor>(d => d.SpecializationId);

            // MedicalHistory
            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.Doctor)
                .WithMany()
                .HasForeignKey(mh => mh.DoctorId);

            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.Patient)
                .WithMany(p => p.MedicalHistories)
                .HasForeignKey(mh => mh.PatientId);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Doctor)
                .WithMany(d => d.Notifications)
                .HasForeignKey(n => n.DoctorId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Patient)
                .WithMany(p => p.Notifications)
                .HasForeignKey(n => n.PatientId);

            // Patient
            modelBuilder.Entity<Patient>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId);

            modelBuilder.Entity<Patient>() // Multiple Medical Histories
                .HasMany(p => p.MedicalHistories)
                .WithOne(mh => mh.Patient)
                .HasForeignKey(mh => mh.PatientId);

            modelBuilder.Entity<Patient>() // Multiple Prescriptions
                .HasMany(p => p.Prescriptions)
                .WithOne(pr => pr.Patient)
                .HasForeignKey(pr => pr.PatientId);

            modelBuilder.Entity<Patient>() // Multiple Notifications
                .HasMany(p => p.Notifications)
                .WithOne(n => n.Patient)
                .HasForeignKey(n => n.PatientId);

            // Prescription
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId);
            
            modelBuilder.Entity<Prescription>() 
                .HasOne(p => p.Patient)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(p => p.PatientId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=../HealthApp.MVC/HealthApp.db");
            }
        }
    }
}