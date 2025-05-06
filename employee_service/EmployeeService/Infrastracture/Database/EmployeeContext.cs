using EmployeeService.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastracture.Database
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Doctor> Doctor { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "employee");
            }).UseSnakeCaseNamingConvention();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("employee");
            modelBuilder.Entity<Employee>().HasKey(e => e.Id);
            modelBuilder.Entity<Doctor>().HasKey(e => e.Id);
            modelBuilder.Entity<Doctor>().HasOne(x => x.Employee)
                .WithOne(x => x.Doctor)
                .HasForeignKey<Employee>();
            modelBuilder.Entity<Employee>().HasQueryFilter(x => !x.IsFired);
        }

    }
}
