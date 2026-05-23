using System.ComponentModel.DataAnnotations;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Course Name")]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Fee Amount")]
    public decimal FeeAmount { get; set; } = Course.DefaultFeeAmount;

    [Required]
    [StringLength(100)]
    public string Duration { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
