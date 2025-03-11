using System.Data.Entity;
using HealthApp.Domain.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.Domain
{
    public class ApplicationDbContext: Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext<User>
    {
        public Microsoft.EntityFrameworkCore.DbSet<Doctor> Doctors { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<Patient> Patients { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<Appointment> Appointments { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<Prescription> Prescriptions { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<MedicalHistory> MedicalHistory { get; set; }

        public ApplicationDbContext() : base("DefaultConnection") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
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
