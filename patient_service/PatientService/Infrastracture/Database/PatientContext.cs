using MassTransit;
using Microsoft.EntityFrameworkCore;
using PatientService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Infrastracture.Database
{
    public class PatientContext: DbContext
    {
        public PatientContext(DbContextOptions<PatientContext> options) : base(options)
        {
       
        }
        public DbSet<Patient> Patients { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "patient");
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("patient");         
            modelBuilder.Entity<Patient>().HasKey(p => p.Id);
            modelBuilder.Entity<Patient>().HasIndex(p => p.PESEL).IsUnique(true);
        }
    }
}
