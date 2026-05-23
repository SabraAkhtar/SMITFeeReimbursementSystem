using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.Models;

public class Course
{
    public const decimal DefaultFeeAmount = 3000m;

    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Course Name")]
    public string CourseName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Display(Name = "Fee Amount")]
    public decimal FeeAmount { get; set; } = DefaultFeeAmount;

    [Required]
    [StringLength(100)]
    public string Duration { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseEnrollment> Enrollments { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
