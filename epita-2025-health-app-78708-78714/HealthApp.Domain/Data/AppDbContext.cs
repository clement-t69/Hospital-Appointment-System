using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.Domain.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<DbContext> options) : base(options) { }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalHistory> MedicalHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Doctor>().ToTable("Doctors");
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<Prescription>().ToTable("Prescriptions");
            modelBuilder.Entity<MedicalHistory>().ToTable("MedicalHistories");
        }
    }
}
