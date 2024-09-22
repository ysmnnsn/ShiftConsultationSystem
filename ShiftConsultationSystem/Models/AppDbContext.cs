namespace ShiftConsultationSystem.Models
{
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ConsultationRequest> ConsultationRequests { get; set; }
        public DbSet<ConsultationAcceptance> ConsultationAcceptances { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define foreign key relationships for ConsultationRequests
            modelBuilder.Entity<ConsultationRequest>()
                .HasOne(c => c.Requester)
                .WithMany()
                .HasForeignKey(c => c.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascading deletes

            modelBuilder.Entity<ConsultationRequest>()
                .HasOne(c => c.Department)
                .WithMany()
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsultationRequest>()
                .HasOne(c => c.Hospital)
                .WithMany()
                .HasForeignKey(c => c.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Define foreign key relationships for ConsultationAcceptances
            modelBuilder.Entity<ConsultationAcceptance>()
                .HasOne(ca => ca.ConsultationRequest)
                .WithMany()
                .HasForeignKey(ca => ca.ConsultationRequestId)
                .OnDelete(DeleteBehavior.Cascade);  // Cascade delete on ConsultationAcceptance

            modelBuilder.Entity<ConsultationAcceptance>()
                .HasOne(ca => ca.Doctor)
                .WithMany()
                .HasForeignKey(ca => ca.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict delete for doctors
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=ARACHNE;Database=ShiftConsultationSystem;User ID=sa; Password=password456;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

}
