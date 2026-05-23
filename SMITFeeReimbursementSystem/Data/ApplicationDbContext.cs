using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(entity =>
        {
            entity.Property(c => c.FeeAmount).HasPrecision(18, 2);
        });

        builder.Entity<CourseEnrollment>(entity =>
        {
            entity.HasIndex(e => new { e.CourseId, e.StudentId }).IsUnique();
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.HasIndex(p => p.TransactionId);
            entity.HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.ReviewedBy)
                .WithMany()
                .HasForeignKey(p => p.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.Receipt)
                .WithOne(r => r.Payment)
                .HasForeignKey<Receipt>(r => r.PaymentId);
        });

        builder.Entity<Receipt>(entity =>
        {
            entity.Property(r => r.Amount).HasPrecision(18, 2);
            entity.HasIndex(r => r.ReceiptNumber).IsUnique();
            entity.HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Course)
                .WithMany()
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.AttendanceId);
            entity.HasIndex(a => new { a.StudentId, a.CourseId, a.Date }).IsUnique();
            entity.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.MarkedBy)
                .WithMany()
                .HasForeignKey(a => a.MarkedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Refund>(entity =>
        {
            entity.HasKey(r => r.RefundId);
            entity.Property(r => r.AttendancePercentage).HasPrecision(5, 2);
            entity.HasIndex(r => new { r.StudentId, r.CourseId }).IsUnique();
            entity.HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Course)
                .WithMany()
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.ReviewedBy)
                .WithMany()
                .HasForeignKey(r => r.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
