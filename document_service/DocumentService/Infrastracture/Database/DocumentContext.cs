using DocumentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.Infrastracture.Database
{
    public class DocumentContext: DbContext
    {
        public DocumentContext(DbContextOptions options) : base(options)
        {
        }

        protected DocumentContext()
        {
        }

        public DbSet<Document> Documents { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "document");
            }).UseSnakeCaseNamingConvention();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("document");
            modelBuilder.Entity<Document>().HasKey(p => p.Id);
            modelBuilder.Entity<Document>().HasIndex(p => p.AppointmentId);
        }
    }
}
