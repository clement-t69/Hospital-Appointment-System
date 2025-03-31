using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.Domain.Data
{
    public class ApplicationDbContext: IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //public DbSet<User> Users { get; set; }
        //public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Doctor
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Doctor>(d => d.UserId);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Notifications)
                .WithOne(n => n.Doctor)
                .HasForeignKey(n => n.DoctorId);

            // MedicalHistory
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
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.MedicalHistories)
                .WithOne(mh => mh.Patient)
                .HasForeignKey(mh => mh.PatientId);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Prescriptions)
                .WithOne(pr => pr.Patient)
                .HasForeignKey(pr => pr.PatientId);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Notifications)
                .WithOne(n => n.Patient)
                .HasForeignKey(n => n.PatientId);

            // Prescription
            modelBuilder.Entity<Prescription>()
                .HasOne(pr => pr.Patient)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(pr => pr.PatientId);
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